using CourierTrack.Domain.Entities;

namespace CourierTrack.Infrastructure.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Order>> GetByCourierIdAsync(Guid courierId);
}
