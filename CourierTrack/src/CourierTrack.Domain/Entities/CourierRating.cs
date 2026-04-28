namespace CourierTrack.Domain.Entities;

public class CourierRating
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CourierId { get; set; }
    public Guid CustomerId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Courier Courier { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public User Customer { get; set; } = null!;
}
