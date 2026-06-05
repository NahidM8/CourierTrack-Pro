namespace CourierTrack.Application.Services;

public class OrderAssignmentService : IOrderAssignmentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly ITrackingHubService _trackingHubService;
    private readonly IMapper _mapper;

    public OrderAssignmentService(
        IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        ITrackingHubService trackingHubService,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _courierRepository = courierRepository;
        _trackingHubService = trackingHubService;
        _mapper = mapper;
    }

    private static Guid SystemUserId => Guid.Empty;

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

        var oldStatus = order.Status;
        order.CourierId = courierId;
        order.Status = OrderStatus.Assigned;
        courier.IsAvailable = false;

        await _orderRepository.UpdateAsync(order);
        await _courierRepository.UpdateAsync(courier);

        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Assigned,
            ChangedBy = SystemUserId,
            Note = $"Auto-assigned to courier {courier.Id}"
        };
        await _orderRepository.AddStatusHistoryAsync(history);

        await _trackingHubService.NotifyNewOrderAssignedAsync(courierId, orderId);

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
        {
            if (order.Status == OrderStatus.Created)
            {
                var history = new OrderStatusHistory
                {
                    OrderId = orderId,
                    OldStatus = OrderStatus.Created,
                    NewStatus = OrderStatus.Pending,
                    ChangedBy = SystemUserId,
                    Note = "No available couriers found. Order moved to pending queue."
                };
                order.Status = OrderStatus.Pending;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.AddStatusHistoryAsync(history);
            }
            return _mapper.Map<OrderDto>(order);
        }

        var maxDeliveries = nearest.Max(x => x.Courier.TotalDeliveries);
        var maxDistance = nearest.Max(x => x.Distance);

        var best = nearest
            .Select(x => new
            {
                x.Courier,
                Score = (0.6m * (x.Courier.Rating ?? 0m)) +
                        (0.4m * (maxDeliveries == 0 ? 0 : (decimal)x.Courier.TotalDeliveries / maxDeliveries)) +
                        (0.2m * (maxDistance == 0 ? 0 : 1 - (x.Distance / maxDistance)))
            })
            .OrderByDescending(x => x.Score)
            .First();

        return await AssignOrderAsync(orderId, best.Courier.Id);
    }

    private static bool IsVehicleSuitable(VehicleType vehicle, decimal weight, PackageSize size)
    {
        if (size == PackageSize.XLarge || weight > ApplicationConstants.Assignment.HeavyPackageWeightThreshold)
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

        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = OrderStatus.Assigned,
            NewStatus = OrderStatus.Pending,
            ChangedBy = SystemUserId,
            Note = $"Unassigned from courier {courier.Id}. Order returned to pending queue."
        };

        order.CourierId = null;
        order.Status = OrderStatus.Pending;
        courier.IsAvailable = true;

        await _orderRepository.UpdateAsync(order);
        await _courierRepository.UpdateAsync(courier);
        await _orderRepository.AddStatusHistoryAsync(history);

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
