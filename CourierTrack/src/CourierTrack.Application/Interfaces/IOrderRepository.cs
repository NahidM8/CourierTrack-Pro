namespace CourierTrack.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> AddAsync(Order entity);
    Task<Order> UpdateAsync(Order entity);
    Task<Order?> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Order>> GetByCourierIdAsync(Guid courierId);
}
