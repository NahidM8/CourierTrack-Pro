namespace CourierTrack.Application.Services;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _courierRepository;

    public CourierService(ICourierRepository courierRepository)
    {
        _courierRepository = courierRepository;
    }
    public Task AcceptOrderAsync(Guid courierId, Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<CourierDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<CourierDto> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderDto>> GetCourierOrdersAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task RejectOrderAsync(Guid courierId, Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAvailabilityAsync(Guid id, bool isAvailable)
    {
        throw new NotImplementedException();
    }

    public Task UpdateLocationAsync(Guid id, double latitude, double longitude)
    {
        throw new NotImplementedException();
    }
}
