namespace CourierTrack.Domain.Entities;

public class CourierRating
{
    public int Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid CourierId { get; init; }
    public Guid CustomerId { get; init; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Courier Courier { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public User Customer { get; set; } = null!;
}
