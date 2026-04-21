namespace CourierTrack.Application.DTOs;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    Role Role,
    DateTime CreatedAt,
    bool IsActive
);

public record UpdateUserStatusDto(bool IsActive);

public record DashboardDto(
    int TotalOrders,
    int ActiveCouriers,
    int TotalUsers,
    decimal TotalRevenue,
    int PendingOrders,
    int DeliveredOrders
);

public record DailyReportDto(
    DateTime Date,
    int TotalOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal Revenue
);

public record RevenueReportItemDto(
    DateTime Date,
    int TotalOrders,
    decimal Revenue
);
