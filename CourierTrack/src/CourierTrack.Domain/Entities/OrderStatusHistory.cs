namespace CourierTrack.Domain.Entities;

public class OrderStatusHistory
{
    public int Id { get; init; }
    public Guid OrderId { get; init; }
    public OrderStatus? OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid ChangedBy { get; set; }
    public string? Note { get; set; }

    public Order? Order { get; set; } = null!;
}
