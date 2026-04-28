namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class CourierRatingRepository(CourierTrackDbContext context) : Repository<CourierRating>(context), ICourierRatingRepository
{
    public async Task<CourierRating?> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.OrderId == orderId);
    }

    public async Task<IEnumerable<CourierRating>> GetByCourierIdAsync(Guid courierId)
    {
        return await _dbSet
            .Where(r => r.CourierId == courierId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourierRating>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
