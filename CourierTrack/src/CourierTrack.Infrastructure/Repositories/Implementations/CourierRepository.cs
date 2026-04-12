namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class CourierRepository(CourierTrackDbContext context) : Repository<Courier>(context), ICourierRepository
{
    public async Task<Courier?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Courier>> GetAvailableAsync()
    {
        return await _dbSet.Where(c => c.IsAvailable).ToListAsync();
    }
}