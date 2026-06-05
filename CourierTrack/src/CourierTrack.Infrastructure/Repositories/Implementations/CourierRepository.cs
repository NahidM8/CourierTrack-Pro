namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class CourierRepository(CourierTrackDbContext context) : Repository<Courier>(context), ICourierRepository
{
    public async Task<PagedResult<Courier>> GetAllPagedAsync(CourierFilterDto filter)
    {
        var query = _dbSet.Include(c => c.User).AsQueryable();

        if (filter.IsAvailable.HasValue)
            query = query.Where(c => c.IsAvailable == filter.IsAvailable);

        if (filter.VehicleType.HasValue)
            query = query.Where(c => c.VehicleType == filter.VehicleType);

        return await query
            .OrderBy(c => c.TotalDeliveries)
            .ToPagedResultAsync(filter.Page, filter.PageSize);
    }

    public async Task<Courier?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Courier>> GetAvailableAsync()
    {
        return await _dbSet.Include(c => c.User).Where(c => c.IsAvailable).ToListAsync();
    }

    public async Task<Courier?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

}