using CourierTrack.Domain.Common;
using CourierTrack.Domain.Enums;

namespace CourierTrack.Domain.Entities;

public class Order : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Guid? CourierId { get; set; }
    public string TrackingNumber { get; set; } = null!;
    public string PickupAddress { get; set; } = null!;
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }
    public string DeliveryAddress { get; set; } = null!;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? PackageDescription { get; set; }
    public decimal PackageWeight { get; set; }
    public PackageSize PackageSize { get; set; }
    public decimal EstimatedDistanceKm { get; set; }
    public string EstimatedDuration { get; set; } = null!;
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public User Customer { get; set; } = null!;
    public Courier? Courier { get; set; }
}
