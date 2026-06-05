namespace CourierTrack.Application.Interfaces.Repositories;

public interface ICourierRepository : IRepository<Courier>
{
    Task<PagedResult<Courier>> GetAllPagedAsync(CourierFilterDto filter);
    Task<Courier?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Courier>> GetAvailableAsync();
}
