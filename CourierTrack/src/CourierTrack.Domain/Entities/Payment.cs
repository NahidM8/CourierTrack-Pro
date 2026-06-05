namespace CourierTrack.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    public Order Order { get; set; } = null!;
}