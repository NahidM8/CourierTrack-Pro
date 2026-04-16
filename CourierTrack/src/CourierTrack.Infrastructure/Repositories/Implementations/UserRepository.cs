namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class UserRepository(CourierTrackDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}
