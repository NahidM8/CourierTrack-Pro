namespace CourierTrack.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> GetByOrderIdAsync(Guid orderId);
    Task<PaymentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto request);
    Task<PaymentDto> ConfirmPaymentAsync(ConfirmPaymentDto request);
    Task HandleWebhookAsync(string payload, string stripeSignature);
    Task<PaymentDto> RefundAsync(Guid paymentId, RefundPaymentDto request);
}
