namespace CourierTrack.Application.Interfaces.Hubs;

public interface ITrackingHubService
{
    Task NotifyOrderStatusUpdatedAsync(Guid orderId, OrderStatus newStatus);
    Task NotifyCourierLocationUpdatedAsync(Guid courierId, double lat, double lng);
    Task NotifyNewOrderAssignedAsync(Guid courierId, Guid orderId);
    Task NotifyOrderPickedUpAsync(Guid orderId);
}
