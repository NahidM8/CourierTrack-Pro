namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class OrderRepository(CourierTrackDbContext context) : Repository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await context.Orders.FirstOrDefaultAsync(o => o.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await context.Orders.Where(o => o.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCourierIdAsync(Guid courierId)
    {
        return await context.Orders.Where(o => o.CourierId == courierId).ToListAsync();
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetStatusHistoryAsync(Guid orderId)
    {
        return await context.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task AddStatusHistoryAsync(OrderStatusHistory history)
    {
        await context.OrderStatusHistories.AddAsync(history);
        await context.SaveChangesAsync();
    }
}
