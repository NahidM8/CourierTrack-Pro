namespace CourierTrack.Application.Interfaces;

public interface ICourierService
{
    Task<CourierDto> GetByIdAsync(Guid id);
    Task<IEnumerable<CourierDto>> GetAllAsync();
    Task UpdateAvailabilityAsync(Guid id, bool isAvailable);
    Task UpdateLocationAsync(Guid id, double latitude, double longitude);
    Task<IEnumerable<OrderDto>> GetCourierOrdersAsync(Guid id);
    Task AcceptOrderAsync(Guid courierId, Guid orderId);
    Task RejectOrderAsync(Guid courierId, Guid orderId);
}
