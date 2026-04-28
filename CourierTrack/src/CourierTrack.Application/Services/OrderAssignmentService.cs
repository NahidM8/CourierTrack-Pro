using CourierTrack.Domain.Exceptions;
using CourierTrack.Application.Utilities;

namespace CourierTrack.Application.Services;

public class OrderAssignmentService : IOrderAssignmentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly IMapper _mapper;

    public OrderAssignmentService(
        IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _courierRepository = courierRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto> AssignOrderAsync(Guid orderId, Guid courierId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        var courier = await _courierRepository.GetByIdAsync(courierId)
            ?? throw new NotFoundException($"Courier with id {courierId} not found.");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Created)
            throw new Domain.Exceptions.InvalidOperationException($"Order cannot be assigned in {order.Status} status.");

        if (!courier.IsAvailable)
            throw new Domain.Exceptions.InvalidOperationException("Courier is not available.");

        order.CourierId = courierId;
        order.Status = OrderStatus.Assigned;
        courier.IsAvailable = false;

        await _orderRepository.UpdateAsync(order);
        await _courierRepository.UpdateAsync(courier);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> AutoAssignOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Created)
            throw new Domain.Exceptions.InvalidOperationException($"Order cannot be auto-assigned in {order.Status} status.");

        var availableCouriers = await _courierRepository.GetAvailableAsync();

        var nearest = availableCouriers
            .Where(c => c.CurrentLatitude.HasValue && c.CurrentLongitude.HasValue)
            .Where(c => IsVehicleSuitable(c.VehicleType, order.PackageWeight, order.PackageSize))
            .Select(c => new
            {
                Courier = c,
                Distance = GeoUtils.CalculateDistanceKm(
                    order.PickupLatitude, order.PickupLongitude,
                    c.CurrentLatitude!.Value, c.CurrentLongitude!.Value)
            })
            .OrderBy(x => x.Distance)
            .Take(5)
            .ToList();

        if (nearest.Count == 0)
            throw new Domain.Exceptions.InvalidOperationException("No available couriers found.");

        var maxDeliveries = nearest.Max(x => x.Courier.TotalDeliveries);

        var best = nearest
            .Select(x => new
            {
                x.Courier,
                Score = (0.6m * (x.Courier.Rating ?? 0m)) +
                        (0.4m * (maxDeliveries == 0 ? 0 : (decimal)x.Courier.TotalDeliveries / maxDeliveries))
            })
            .OrderByDescending(x => x.Score)
            .First();

        return await AssignOrderAsync(orderId, best.Courier.Id);
    }

    private static bool IsVehicleSuitable(VehicleType vehicle, decimal weight, PackageSize size)
    {
        if (size == PackageSize.XLarge || weight > 20)
            return vehicle is VehicleType.Car or VehicleType.Van;
        return true;
    }

    public async Task<OrderDto> UnassignOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.Status != OrderStatus.Assigned)
            throw new Domain.Exceptions.InvalidOperationException($"Only assigned orders can be unassigned.");

        if (!order.CourierId.HasValue)
            throw new Domain.Exceptions.InvalidOperationException("Order is not assigned to any courier.");

        var courier = await _courierRepository.GetByIdAsync(order.CourierId.Value)
            ?? throw new NotFoundException($"Courier with id {order.CourierId.Value} not found.");

        if (courier.IsAvailable)
            throw new Domain.Exceptions.InvalidOperationException("Courier is already available. Cannot unassign an order from an available courier.");

        order.CourierId = null;
        order.Status = OrderStatus.Pending;
        courier.IsAvailable = true;

        await _orderRepository.UpdateAsync(order);
        await _courierRepository.UpdateAsync(courier);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetUnassignedOrdersAsync()
    {
        var orders = await _orderRepository.GetUnassignedOrdersAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<bool> IsOrderAssignedAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        return order.Status == OrderStatus.Assigned && order.CourierId.HasValue;
    }
}
