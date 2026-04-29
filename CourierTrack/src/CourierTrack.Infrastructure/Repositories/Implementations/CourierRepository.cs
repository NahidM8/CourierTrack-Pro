using CourierTrack.Application.DTOs;
using CourierTrack.Domain.Common;

namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class CourierRepository(CourierTrackDbContext context) : Repository<Courier>(context), ICourierRepository
{
    public async Task<PagedResult<Courier>> GetAllPagedAsync(CourierFilterDto filter)
    {
        var query = _dbSet.AsQueryable();

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
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Courier>> GetAvailableAsync()
    {
        return await _dbSet.Where(c => c.IsAvailable).ToListAsync();
    }

}