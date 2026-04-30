using CourierTrack.Domain.Constants;

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
            .Group($"{ApplicationConstants.Groups.OrderGroupPrefix}{orderId}")
            .SendAsync(ApplicationConstants.Hubs.OrderStatusUpdated, orderId, newStatus.ToString());
    }

    public async Task NotifyCourierLocationUpdatedAsync(Guid courierId, double lat, double lng)
    {
        await _hubContext.Clients
            .Group($"{ApplicationConstants.Groups.OrderGroupPrefix}{courierId}")
            .SendAsync(ApplicationConstants.Hubs.CourierLocationUpdated, courierId, lat, lng);
    }

    public async Task NotifyNewOrderAssignedAsync(Guid courierId, Guid orderId)
    {
        await _hubContext.Clients
            .Group($"{ApplicationConstants.Groups.CourierGroupPrefix}{courierId}")
            .SendAsync(ApplicationConstants.Hubs.NewOrderAssigned, orderId);
    }

    public async Task NotifyOrderPickedUpAsync(Guid orderId)
    {
        await _hubContext.Clients
            .Group($"{ApplicationConstants.Groups.OrderGroupPrefix}{orderId}")
            .SendAsync(ApplicationConstants.Hubs.OrderPickedUp, orderId);
    }
}
