namespace CourierTrack.Domain.Entities;

public class Courier : BaseEntity
{
    public Guid UserId { get; init; }
    public VehicleType VehicleType { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? Rating { get; set; }
    public int TotalDeliveries { get; set; }

    public User User { get; set; } = null!;
    public ICollection<CourierRating> Ratings { get; set; } = [];
}
