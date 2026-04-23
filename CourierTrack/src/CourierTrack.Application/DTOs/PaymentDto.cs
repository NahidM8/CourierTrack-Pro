namespace CourierTrack.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentStatus Status,
    string? StripePaymentIntentId,
    DateTime? PaidAt
);

public record CreatePaymentIntentDto(
    Guid OrderId,
    PaymentMethod PaymentMethod,
    string Currency
);

public record ConfirmPaymentDto(
    string StripePaymentIntentId
);

public record RefundPaymentDto(
    string Reason
);
