namespace CourierTrack.Application.DTOs;

public record CourierRatingDto(
    int Id,
    Guid OrderId,
    Guid CourierId,
    Guid CustomerId,
    int Score,
    string? Comment,
    DateTime CreatedAt
);
