namespace CourierTrack.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string TrackingNubmer { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
