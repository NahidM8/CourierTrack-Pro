namespace CourierTrack.Application.DTOs;

public record OrderStatusHistoryDto(
    Guid Id,
    Guid OrderId,
    OrderStatus? OldStatus,
    OrderStatus NewStatus,
    DateTime ChangedAt,
    Guid ChangedBy,
    string? Note
);
