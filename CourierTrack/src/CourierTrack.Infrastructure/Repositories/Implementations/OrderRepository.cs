using CourierTrack.Domain.Entities;
using CourierTrack.Infrastructure.Data.Context;
using CourierTrack.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class OrderRepository(CourierTrackDbContext context) : Repository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(o => o.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCourierIdAsync(Guid courierId)
    {
        return await _dbSet.Where(o => o.CourierId == courierId).ToListAsync();
    }
}
