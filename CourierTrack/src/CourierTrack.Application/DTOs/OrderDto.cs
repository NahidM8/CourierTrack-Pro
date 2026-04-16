namespace CourierTrack.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    Guid? CourierId,
    string TrackingNumber,
    string PickupAddress,
    double PickupLatitude,
    double PickupLongitude,
    string DeliveryAddress,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string? PackageDescription,
    decimal PackageWeight,
    PackageSize PackageSize,
    decimal EstimatedDistanceKm,
    string EstimatedDuration,
    decimal Price,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt
    );

public record CreateOrderDto(
    Guid CustomerId,
    string PickupAddress,
    double PickupLatitude,
    double PickupLongitude,
    string DeliveryAddress,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string? PackageDescription,
    decimal PackageWeight,
    PackageSize PackageSize
    );

public record UpdateOrderDto(
    Guid? CourierId,
    OrderStatus Status,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt
    );
