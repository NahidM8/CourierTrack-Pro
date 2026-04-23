using CourierTrack.Domain.Exceptions;
using Microsoft.Extensions.Options;
using Stripe;

namespace CourierTrack.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly StripeOptions _stripeOptions;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IMapper mapper,
        IOptions<StripeOptions> stripeOptions)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
        _stripeOptions = stripeOptions.Value;
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
    }

    public async Task<PaymentDto> GetByOrderIdAsync(Guid orderId)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId)
            ?? throw new NotFoundException($"Payment for order {orderId} not found.");

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto request)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId)
            ?? throw new NotFoundException($"Order with id {request.OrderId} not found.");

        if (order.Status == OrderStatus.Cancelled)
            throw new BadRequestException("Cannot create payment for a cancelled order.");

        var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
        if (existingPayment is not null && existingPayment.Status == PaymentStatus.Completed)
            throw new BadRequestException("Order has already been paid.");

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(order.Price * 100),
            Currency = request.Currency.ToLower(),
            PaymentMethodTypes = ["card"],
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            Amount = order.Price,
            Currency = request.Currency.ToUpper(),
            PaymentMethod = request.PaymentMethod,
            StripePaymentIntentId = paymentIntent.Id,
            Status = PaymentStatus.Pending
        };

        var created = await _paymentRepository.AddAsync(payment);
        return _mapper.Map<PaymentDto>(created);
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(ConfirmPaymentDto request)
    {
        var payment = await _paymentRepository.GetByStripePaymentIntentIdAsync(request.StripePaymentIntentId)
            ?? throw new NotFoundException($"Payment with intent {request.StripePaymentIntentId} not found.");

        if (payment.Status == PaymentStatus.Completed)
            throw new BadRequestException("Payment has already been completed.");

        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(request.StripePaymentIntentId);

        if (paymentIntent.Status != "succeeded")
            throw new BadRequestException($"Payment intent is not succeeded. Current status: {paymentIntent.Status}");

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;

        var updated = await _paymentRepository.UpdateAsync(payment);
        return _mapper.Map<PaymentDto>(updated);
    }

    public async Task HandleWebhookAsync(string payload, string stripeSignature)
    {
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                stripeSignature,
                _stripeOptions.WebhookSecret
            );
        }
        catch (StripeException)
        {
            throw new BadRequestException("Invalid Stripe webhook signature.");
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent is null) break;

                    var payment = await _paymentRepository.GetByStripePaymentIntentIdAsync(paymentIntent.Id);
                    if (payment is null) break;

                    payment.Status = PaymentStatus.Completed;
                    payment.PaidAt = DateTime.UtcNow;
                    await _paymentRepository.UpdateAsync(payment);
                    break;
                }
            case "payment_intent.payment_failed":
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent is null) break;

                    var payment = await _paymentRepository.GetByStripePaymentIntentIdAsync(paymentIntent.Id);
                    if (payment is null) break;

                    payment.Status = PaymentStatus.Failed;
                    await _paymentRepository.UpdateAsync(payment);
                    break;
                }
        }
    }

    public async Task<PaymentDto> RefundAsync(Guid paymentId, RefundPaymentDto request)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId)
            ?? throw new NotFoundException($"Payment with id {paymentId} not found.");

        if (payment.Status != PaymentStatus.Completed)
            throw new BadRequestException("Only completed payments can be refunded.");

        var options = new RefundCreateOptions
        {
            PaymentIntent = payment.StripePaymentIntentId,
            Reason = "requested_by_customer"
        };

        var service = new RefundService();
        await service.CreateAsync(options);

        payment.Status = PaymentStatus.Refunded;
        var updated = await _paymentRepository.UpdateAsync(payment);
        return _mapper.Map<PaymentDto>(updated);
    }
}
