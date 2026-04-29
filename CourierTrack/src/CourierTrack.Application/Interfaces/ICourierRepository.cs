using CourierTrack.Domain.Common;

namespace CourierTrack.Application.Interfaces;

public interface ICourierRepository : IRepository<Courier>
{
    Task<PagedResult<Courier>> GetAllPagedAsync(CourierFilterDto filter);
    Task<Courier?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Courier>> GetAvailableAsync();
}
