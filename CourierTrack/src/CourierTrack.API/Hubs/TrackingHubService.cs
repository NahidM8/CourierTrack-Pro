namespace CourierTrack.API.Hubs;

public class TrackingHubService : ITrackingHubService
{
    private readonly IHubContext<TrackingHub> _hubContext;

    public TrackingHubService(IHubContext<TrackingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderStatusUpdatedAsync(Guid orderId, OrderStatus newStatus)
    {
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderStatusUpdated", orderId, newStatus.ToString());
    }

    public async Task NotifyCourierLocationUpdatedAsync(Guid courierId, double lat, double lng)
    {
        await _hubContext.Clients
            .Group($"order-{courierId}")
            .SendAsync("CourierLocationUpdated", courierId, lat, lng);
    }

    public async Task NotifyNewOrderAssignedAsync(Guid courierId, Guid orderId)
    {
        await _hubContext.Clients
            .Group($"courier-{courierId}")
            .SendAsync("NewOrderAssigned", orderId);
    }

    public async Task NotifyOrderPickedUpAsync(Guid orderId)
    {
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderPickedUp", orderId);
    }
}
