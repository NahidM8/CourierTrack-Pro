namespace CourierTrack.Application.Services;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _courierRepository;
    private readonly IMapper _mapper;

    public CourierService(ICourierRepository courierRepository, IMapper mapper)
    {
        _courierRepository = courierRepository;
        _mapper = mapper;
    }
    public Task AcceptOrderAsync(Guid courierId, Guid orderId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<CourierDto>> GetAllAsync()
    {
        var couriers = await _courierRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourierDto>>(couriers);
    }

    public async Task<CourierDto> GetByIdAsync(Guid id)
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
