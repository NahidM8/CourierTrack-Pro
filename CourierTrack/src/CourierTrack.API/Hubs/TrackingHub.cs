namespace CourierTrack.API.Hubs;

[Authorize]
public class TrackingHub : Hub
{
    public async Task JoinOrderGroup(string orderId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"{ApplicationConstants.Groups.OrderGroupPrefix}{orderId}");

    public async Task LeaveOrderGroup(string orderId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{ApplicationConstants.Groups.OrderGroupPrefix}{orderId}");

    public async Task JoinCourierGroup(string courierId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"{ApplicationConstants.Groups.CourierGroupPrefix}{courierId}");

    public async Task LeaveCourierGroup(string courierId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{ApplicationConstants.Groups.CourierGroupPrefix}{courierId}");

    [Authorize(Roles = "Admin")]
    public async Task JoinAdminGroup()
        => await Groups.AddToGroupAsync(Context.ConnectionId, ApplicationConstants.Groups.AdminGroup);

    [Authorize(Roles = "Admin")]
    public async Task LeaveAdminGroup()
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, ApplicationConstants.Groups.AdminGroup);
}

