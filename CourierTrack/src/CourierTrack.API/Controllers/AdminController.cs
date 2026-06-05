namespace CourierTrack.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    IUserService userService,
    IOrderService orderService,
    ICourierService courierService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResult(users));
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResult(user));
    }

    [HttpPut("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusDto dto)
    {
        await userService.UpdateStatusAsync(id, dto.IsActive);
        return Ok(ApiResponse<object>.SuccessResult("Updated User status successfully"));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var orders = await orderService.GetAllAsync();
        var couriers = await courierService.GetAllAsync();
        var users = await userService.GetAllAsync();

        var dashboard = new DashboardDto(
            TotalOrders: orders.Count(),
            ActiveCouriers: couriers.Count(c => c.IsAvailable),
            TotalUsers: users.Count(),
            TotalRevenue: orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.Price),
            PendingOrders: orders.Count(o => o.Status == OrderStatus.Pending),
            DeliveredOrders: orders.Count(o => o.Status == OrderStatus.Delivered)
        );

        return Ok(ApiResponse<DashboardDto>.SuccessResult(dashboard));
    }

    [HttpGet("reports/daily")]
    public async Task<IActionResult> GetDailyReport()
    {
        var orders = await orderService.GetAllAsync();
        var today = DateTime.UtcNow.Date;

        var dailyReport = new DailyReportDto(
            Date: today,
            TotalOrders: orders.Count(o => o.CreatedAt.Date == today),
            DeliveredOrders: orders.Count(o => o.DeliveredAt.HasValue && o.DeliveredAt.Value.Date == today),
            CancelledOrders: orders.Count(o => o.Status == OrderStatus.Cancelled && o.CreatedAt.Date == today),
            Revenue: orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt.Date == today)
                .Sum(o => o.Price)
        );

        return Ok(ApiResponse<DailyReportDto>.SuccessResult(dailyReport));
    }

    [HttpGet("reports/revenue")]
    public async Task<IActionResult> GetRevenueReport()
    {
        var orders = await orderService.GetAllAsync();

        var revenueReport = orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueReportItemDto(
                Date: g.Key,
                TotalOrders: g.Count(),
                Revenue: g.Sum(o => o.Price)
            ))
            .OrderByDescending(r => r.Date);

        return Ok(ApiResponse<IEnumerable<RevenueReportItemDto>>.SuccessResult(revenueReport));
    }
}
