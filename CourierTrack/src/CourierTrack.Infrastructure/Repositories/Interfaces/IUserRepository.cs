using CourierTrack.Domain.Entities;

namespace CourierTrack.Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
