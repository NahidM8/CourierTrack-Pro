namespace CourierTrack.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Order>> GetByCourierIdAsync(Guid courierId);
    Task<IEnumerable<OrderStatusHistory>> GetStatusHistoryAsync(Guid orderId);
    Task AddStatusHistoryAsync(OrderStatusHistory history);
    Task<IEnumerable<Order>> GetUnassignedOrdersAsync();
}
