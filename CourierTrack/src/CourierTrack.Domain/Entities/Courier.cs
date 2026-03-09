using CourierTrack.Domain.Enums;

namespace CourierTrack.Domain.Entities;

public class Courier
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public VehicleType VehicleType { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? Rating { get; set; }
    public int TotalDeliveries { get; set; }

    public User User { get; set; } = null!;
}
