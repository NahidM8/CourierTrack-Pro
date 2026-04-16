namespace CourierTrack.Application.Interfaces;

public interface ICourierRepository : IRepository<Courier>
{
    Task<Courier?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Courier>> GetAvailableAsync();
}
