using Stripe;

namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class OrderRepository(CourierTrackDbContext context) : Repository<Order>(context), IOrderRepository
{
    public async Task<PagedResult<Order>> GetAllPagedAsync(OrderFilterDto filter)
    {
        var query = _dbSet.AsQueryable();

        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId);

        if (filter.CourierId.HasValue)
            query = query.Where(o => o.CourierId == filter.CourierId);

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToPagedResultAsync(filter.Page, filter.PageSize);
    }

    public async Task<Order?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await context.Orders.FirstOrDefaultAsync(o => o.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCourierIdAsync(Guid courierId)
    {
        return await _dbSet.Where(o => o.CourierId == courierId).ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetUnassignedOrdersAsync()
    {
        return await context.Orders
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Created)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
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
