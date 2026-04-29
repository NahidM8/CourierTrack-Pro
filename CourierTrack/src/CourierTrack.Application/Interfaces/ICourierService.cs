using CourierTrack.Domain.Common;

namespace CourierTrack.Application.Interfaces;

public interface ICourierService
{
    Task<PagedResult<CourierDto>> GetAllPagedAsync(CourierFilterDto filter);
    Task<IEnumerable<CourierDto>> GetAllAsync();
    Task<CourierDto> GetByIdAsync(Guid id);
    Task<CourierDto> GetByUserIdAsync(Guid userId);
    Task UpdateAvailabilityAsync(Guid id, bool isAvailable);
    Task UpdateLocationAsync(Guid id, double latitude, double longitude);
    Task<IEnumerable<OrderDto>> GetCourierOrdersAsync(Guid id);
    Task AcceptOrderAsync(Guid courierId, Guid orderId);
    Task RejectOrderAsync(Guid courierId, Guid orderId);
}
