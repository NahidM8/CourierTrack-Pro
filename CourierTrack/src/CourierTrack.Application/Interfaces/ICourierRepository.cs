using CourierTrack.Domain.Entities;

namespace CourierTrack.Infrastructure.Repositories.Interfaces;

public interface ICourierRepository : IRepository<Courier>
{
    Task<Courier?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Courier>> GetAvailableAsync();
}
