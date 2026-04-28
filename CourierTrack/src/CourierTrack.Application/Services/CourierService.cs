using CourierTrack.Domain.Exceptions;

namespace CourierTrack.Application.Services;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _courierRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public CourierService(ICourierRepository courierRepository, IOrderRepository orderRepository, IMapper mapper)
    {
        _courierRepository = courierRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourierDto>> GetAllAsync()
    {
        var couriers = await _courierRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourierDto>>(couriers);
    }

    public async Task<CourierDto> GetByIdAsync(Guid id)
    {
        var courier = await _courierRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Courier with id {id} not found.");

        return _mapper.Map<CourierDto>(courier);
    }

    public async Task<CourierDto> GetByUserIdAsync(Guid userId)
    {
        var courier = await _courierRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException($"Courier profile not found for user {userId}.");

        return _mapper.Map<CourierDto>(courier);
    }

    public async Task UpdateAvailabilityAsync(Guid id, bool isAvailable)
    {
        var courier = await _courierRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Courier with id {id} not found.");

        courier.IsAvailable = isAvailable;
        await _courierRepository.UpdateAsync(courier);
    }

    public async Task UpdateLocationAsync(Guid id, double latitude, double longitude)
    {
        var courier = await _courierRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Courier with id {id} not found.");

        courier.CurrentLatitude = latitude;
        courier.CurrentLongitude = longitude;
        await _courierRepository.UpdateAsync(courier);
    }

    public async Task<IEnumerable<OrderDto>> GetCourierOrdersAsync(Guid id)
    {
        var courier = await _courierRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Courier with id {id} not found.");

        var orders = await _orderRepository.GetByCourierIdAsync(courier.Id);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task AcceptOrderAsync(Guid courierId, Guid orderId)
    {
        var courier = await _courierRepository.GetByIdAsync(courierId)
            ?? throw new NotFoundException($"Courier with id {courierId} not found.");

        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.Status != OrderStatus.Assigned)
            throw new Domain.Exceptions.InvalidOperationException($"Order is not in an assigned state.");

        order.CourierId = courierId;
        order.Status = OrderStatus.Assigned;
        courier.IsAvailable = false;

        await _orderRepository.UpdateAsync(order);
        await _courierRepository.UpdateAsync(courier);
    }

    public async Task RejectOrderAsync(Guid courierId, Guid orderId)
    {
        var courier = await _courierRepository.GetByIdAsync(courierId)
            ?? throw new NotFoundException($"Courier with id {courierId} not found.");

        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.Status != OrderStatus.Assigned || order.CourierId != courierId)
            throw new Domain.Exceptions.InvalidOperationException("This order is not assigned to this courier.");

        order.CourierId = null;
        order.Status = OrderStatus.Pending;
        courier.IsAvailable = true;

        await _orderRepository.UpdateAsync(order);
        await _courierRepository.UpdateAsync(courier);
    }
}