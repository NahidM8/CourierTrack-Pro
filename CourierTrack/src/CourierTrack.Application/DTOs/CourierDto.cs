namespace CourierTrack.Application.DTOs;

public record CourierDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string PhoneNumber,
    VehicleType VehicleType,
    double? CurrentLatitude,
    double? CurrentLongitude,
    bool IsAvailable,
    decimal? Rating,
    int TotalDeliveries
);