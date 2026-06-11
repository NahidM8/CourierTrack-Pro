namespace CourierTrack.API.Hubs;

public class TrackingHubService(
    IHubContext<TrackingHub> hubContext,
    IOrderRepository orderRepository) : ITrackingHubService
{
    private static readonly OrderStatus[] ActiveDeliveryStatuses =
    [
        OrderStatus.Assigned,
        OrderStatus.PickedUp,
        OrderStatus.InTransit
    ];

    public async Task NotifyOrderStatusUpdatedAsync(Guid orderId, OrderStatus newStatus)
    {
        var message = new OrderStatusUpdateMessage(orderId, newStatus.ToString());

        await hubContext.Clients
            .Group($"{ApplicationConstants.Groups.OrderGroupPrefix}{orderId}")
            .SendAsync(ApplicationConstants.Hubs.OrderStatusUpdated, message);
    }

    public async Task NotifyCourierLocationUpdatedAsync(Guid courierId, double lat, double lng)
    {
        var location = new LocationPoint(lat, lng);
        var courierMessage = new LocationUpdateMessage(courierId, lat, lng, location);

        await hubContext.Clients
            .Group($"{ApplicationConstants.Groups.CourierGroupPrefix}{courierId}")
            .SendAsync(ApplicationConstants.Hubs.CourierLocationUpdated, courierMessage);

        await hubContext.Clients
            .Group(ApplicationConstants.Groups.AdminGroup)
            .SendAsync(ApplicationConstants.Hubs.CourierLocationUpdated, courierMessage);

        var activeOrders = await orderRepository.GetByCourierIdAsync(courierId);
        foreach (var order in activeOrders.Where(o => ActiveDeliveryStatuses.Contains(o.Status)))
        {
            var orderMessage = new OrderLocationUpdateMessage(order.Id, courierId, location);

            await hubContext.Clients
                .Group($"{ApplicationConstants.Groups.OrderGroupPrefix}{order.Id}")
                .SendAsync(ApplicationConstants.Hubs.CourierLocationUpdated, orderMessage);
        }
    }

    public async Task NotifyNewOrderAssignedAsync(Guid courierId, Guid orderId)
    {
        var message = new NewOrderAssignedMessage(courierId, orderId);

        await hubContext.Clients
            .Group($"{ApplicationConstants.Groups.CourierGroupPrefix}{courierId}")
            .SendAsync(ApplicationConstants.Hubs.NewOrderAssigned, message);
    }

    public async Task NotifyOrderPickedUpAsync(Guid orderId)
    {
        var message = new OrderPickedUpMessage(orderId);

        await hubContext.Clients
            .Group($"{ApplicationConstants.Groups.OrderGroupPrefix}{orderId}")
            .SendAsync(ApplicationConstants.Hubs.OrderPickedUp, message);
    }
}
