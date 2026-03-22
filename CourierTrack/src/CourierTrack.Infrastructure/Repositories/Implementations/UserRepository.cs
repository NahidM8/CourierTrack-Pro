using CourierTrack.Domain.Entities;
using CourierTrack.Infrastructure.Data.Context;
using CourierTrack.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class UserRepository(CourierTrackDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}
