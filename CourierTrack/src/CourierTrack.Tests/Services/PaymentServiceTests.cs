using AutoMapper;
using CourierTrack.Application.DTOs;
using CourierTrack.Application.Interfaces.Repositories;
using CourierTrack.Application.Interfaces.Services;
using CourierTrack.Application.Options;
using CourierTrack.Application.Services;
using CourierTrack.Domain.Entities;
using CourierTrack.Domain.Enums;
using CourierTrack.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CourierTrack.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly IPaymentService _paymentService;
    private readonly StripeOptions _stripeOptions;

    public PaymentServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockMapper = new Mock<IMapper>();

        _stripeOptions = new StripeOptions
        {
            SecretKey = "sk_test_123456",
            PublishableKey = "pk_test_123456",
            WebhookSecret = "whsec_test_123456"
        };

        var optionsMonitor = Options.Create(_stripeOptions);

        _paymentService = new PaymentService(
            _mockPaymentRepository.Object,
            _mockOrderRepository.Object,
            _mockMapper.Object,
            optionsMonitor
        );
    }

    #region CreatePaymentIntentAsync Tests

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldThrow_WhenOrderNotFound()
    {
        var request = new CreatePaymentIntentDto(
            OrderId: Guid.NewGuid(),
            Currency: "USD",
            PaymentMethod: PaymentMethod.CreditCard
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(request.OrderId))
            .ReturnsAsync((Order?)null);

        var act = async () => await _paymentService.CreatePaymentIntentAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldThrow_WhenOrderIsCancelled()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Cancelled,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var request = new CreatePaymentIntentDto(
            OrderId: orderId,
            Currency: "USD",
            PaymentMethod: PaymentMethod.CreditCard
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var act = async () => await _paymentService.CreatePaymentIntentAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Cannot create payment for a cancelled order.");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldThrow_WhenPaymentAlreadyCompleted()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Created,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var existingPayment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 15m,
            Currency = "USD",
            PaymentMethod = PaymentMethod.CreditCard,
            StripePaymentIntentId = "pi_completed_123",
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow
        };

        var request = new CreatePaymentIntentDto(
            OrderId: orderId,
            Currency: "USD",
            PaymentMethod: PaymentMethod.CreditCard
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockPaymentRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(existingPayment);

        var act = async () => await _paymentService.CreatePaymentIntentAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Order has already been paid.");
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_ShouldReturnExisting_WhenPendingPaymentExists()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Created,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var existingPayment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 15m,
            Currency = "USD",
            PaymentMethod = PaymentMethod.CreditCard,
            StripePaymentIntentId = "pi_pending_123",
            Status = PaymentStatus.Pending
        };

        var paymentDto = new PaymentDto(
            Id: existingPayment.Id,
            OrderId: existingPayment.OrderId,
            Amount: existingPayment.Amount,
            Currency: existingPayment.Currency,
            PaymentMethod: existingPayment.PaymentMethod,
            StripePaymentIntentId: existingPayment.StripePaymentIntentId,
            Status: existingPayment.Status,
            PaidAt: existingPayment.PaidAt
        );

        var request = new CreatePaymentIntentDto(
            OrderId: orderId,
            Currency: "USD",
            PaymentMethod: PaymentMethod.CreditCard
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockPaymentRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(existingPayment);

        _mockMapper
            .Setup(x => x.Map<PaymentDto>(existingPayment))
            .Returns(paymentDto);

        var result = await _paymentService.CreatePaymentIntentAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be(existingPayment.Id);
        result.Status.Should().Be(PaymentStatus.Pending);
        // Verify we didn't create a new payment intent
        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    #endregion

    #region ConfirmPaymentAsync Tests

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldThrow_WhenPaymentNotFound()
    {
        var request = new ConfirmPaymentDto(
            StripePaymentIntentId: "pi_not_found"
        );

        _mockPaymentRepository
            .Setup(x => x.GetByStripePaymentIntentIdAsync(request.StripePaymentIntentId))
            .ReturnsAsync((Payment?)null);

        var act = async () => await _paymentService.ConfirmPaymentAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldReturnIdempotent_WhenAlreadyCompleted()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var stripeIntentId = "pi_succeeded_123";

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Amount = 15m,
            Currency = "USD",
            PaymentMethod = PaymentMethod.CreditCard,
            StripePaymentIntentId = stripeIntentId,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow.AddHours(-1)
        };

        var paymentDto = new PaymentDto(
            Id: payment.Id,
            OrderId: payment.OrderId,
            Amount: payment.Amount,
            Currency: payment.Currency,
            PaymentMethod: payment.PaymentMethod,
            StripePaymentIntentId: payment.StripePaymentIntentId,
            Status: payment.Status,
            PaidAt: payment.PaidAt
        );

        var request = new ConfirmPaymentDto(
            StripePaymentIntentId: stripeIntentId
        );

        _mockPaymentRepository
            .Setup(x => x.GetByStripePaymentIntentIdAsync(stripeIntentId))
            .ReturnsAsync(payment);

        _mockMapper
            .Setup(x => x.Map<PaymentDto>(payment))
            .Returns(paymentDto);

        var result = await _paymentService.ConfirmPaymentAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Completed);
        result.PaidAt.Should().NotBeNull();
        // Should NOT attempt to update since already completed
        _mockPaymentRepository.Verify(x => x.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldUpdateOrderStatus_WhenPaymentSucceeds()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var stripeIntentId = "pi_succeeded_123";

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Amount = 15m,
            Currency = "USD",
            PaymentMethod = PaymentMethod.CreditCard,
            StripePaymentIntentId = stripeIntentId,
            Status = PaymentStatus.Pending
        };

        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Created,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var updatedPayment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Amount = 15m,
            Currency = "USD",
            PaymentMethod = PaymentMethod.CreditCard,
            StripePaymentIntentId = stripeIntentId,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow
        };

        var paymentDto = new PaymentDto(
            Id: updatedPayment.Id,
            OrderId: updatedPayment.OrderId,
            Amount: updatedPayment.Amount,
            Currency: updatedPayment.Currency,
            PaymentMethod: updatedPayment.PaymentMethod,
            StripePaymentIntentId: updatedPayment.StripePaymentIntentId,
            Status: updatedPayment.Status,
            PaidAt: updatedPayment.PaidAt
        );

        var request = new ConfirmPaymentDto(
            StripePaymentIntentId: stripeIntentId
        );

        _mockPaymentRepository
            .Setup(x => x.GetByStripePaymentIntentIdAsync(stripeIntentId))
            .ReturnsAsync(payment);

        _mockPaymentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync(updatedPayment);

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        _mockMapper
            .Setup(x => x.Map<PaymentDto>(updatedPayment))
            .Returns(paymentDto);

        var result = await _paymentService.ConfirmPaymentAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Completed);
        _mockPaymentRepository.Verify(x => x.UpdateAsync(It.IsAny<Payment>()), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    #endregion
}
