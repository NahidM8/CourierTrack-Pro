namespace CourierTrack.API.Hubs;

public class TrackingHub : Hub
{
    public async Task JoinOrderGroup(string orderId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

    public async Task LeaveOrderGroup(string orderId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");

    public async Task JoinCourierGroup(string courierId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"courier-{courierId}");

    public async Task LeaveCourierGroup(string courierId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"courier-{courierId}");
}
