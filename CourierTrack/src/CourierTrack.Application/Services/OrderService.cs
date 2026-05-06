namespace CourierTrack.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITrackingHubService _trackingHubService;
    private readonly IMapper _mapper;
    private readonly PricingOptions _pricingOptions;

    public OrderService(
        IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        ITrackingHubService trackingHubService,
        IMapper mapper,
        IOptions<PricingOptions> pricingOptions)
    {
        _orderRepository = orderRepository;
        _trackingHubService = trackingHubService;
        _mapper = mapper;
        _pricingOptions = pricingOptions.Value;
    }

    public async Task<PagedResult<OrderDto>> GetAllPagedAsync(OrderFilterDto filter)
    {
        var pagedOrders = await _orderRepository.GetAllPagedAsync(filter);
        return new PagedResult<OrderDto>
        {
            Data = _mapper.Map<IEnumerable<OrderDto>>(pagedOrders.Data),
            Page = pagedOrders.Page,
            PageSize = pagedOrders.PageSize,
            TotalCount = pagedOrders.TotalCount
        };
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order with id {id} not found.");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> GetByTrackingNumberAsync(string trackingNumber)
    {
        var order = await _orderRepository.GetByTrackingNumberAsync(trackingNumber)
            ?? throw new NotFoundException($"Order with tracking number {trackingNumber} not found.");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetByCourierIdAsync(Guid courierId)
    {
        var orders = await _orderRepository.GetByCourierIdAsync(courierId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto request)
    {
        var estimatedDistanceKm = GeoUtils.CalculateDistanceKm(
            request.PickupLatitude,
            request.PickupLongitude,
            request.DeliveryLatitude,
            request.DeliveryLongitude);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CourierId = null,
            TrackingNumber = GenerateTrackingNumber(),
            PickupAddress = request.PickupAddress,
            PickupLatitude = request.PickupLatitude,
            PickupLongitude = request.PickupLongitude,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryLatitude = request.DeliveryLatitude,
            DeliveryLongitude = request.DeliveryLongitude,
            PackageDescription = request.PackageDescription,
            PackageWeight = request.PackageWeight,
            PackageSize = request.PackageSize,
            EstimatedDistanceKm = estimatedDistanceKm,
            EstimatedDuration = CalculateEstimatedDuration(estimatedDistanceKm),
            Price = CalculatePrice(estimatedDistanceKm, request.PackageWeight, request.PackageSize, _pricingOptions),
            Status = OrderStatus.Created
        };

        var createdOrder = await _orderRepository.AddAsync(order);
        return _mapper.Map<OrderDto>(createdOrder);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderDto request, Guid changedBy)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.Status == OrderStatus.Cancelled)
            throw new Domain.Exceptions.InvalidOperationException("Cancelled orders cannot be updated.");

        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = order.Status,
            NewStatus = request.Status,
            ChangedBy = changedBy,
            Note = request.Note
        };

        order.Status = request.Status;

        if (order.Status == OrderStatus.PickedUp) 
        {
            order.PickedUpAt ??= DateTime.UtcNow;
            await _trackingHubService.NotifyOrderPickedUpAsync(order.Id);
        }

        if (order.Status == OrderStatus.Delivered)
            order.DeliveredAt ??= DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.AddStatusHistoryAsync(history);

        await _trackingHubService.NotifyOrderStatusUpdatedAsync(order.Id, order.Status);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> CancelOrderAsync(Guid orderId, Guid changedBy)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.Status == OrderStatus.Delivered)
            throw new Domain.Exceptions.InvalidOperationException("Delivered orders cannot be cancelled.");

        if (order.Status == OrderStatus.Cancelled)
            throw new Domain.Exceptions.InvalidOperationException("Order is already cancelled.");

        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = order.Status,
            NewStatus = OrderStatus.Cancelled,
            ChangedBy = changedBy,
            Note = ApplicationConstants.Order.CancellationReason
        };

        order.Status = OrderStatus.Cancelled;

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.AddStatusHistoryAsync(history);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderStatusHistoryDto>> GetStatusHistoryAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        var history = await _orderRepository.GetStatusHistoryAsync(order.Id);
        return _mapper.Map<IEnumerable<OrderStatusHistoryDto>>(history);
    }

    private static string GenerateTrackingNumber()
    {
        var randomNumber = Guid.NewGuid().ToString("N")[..ApplicationConstants.Order.TrackingNumberRandomLength].ToUpperInvariant();
        return $"{ApplicationConstants.Order.TrackingNumberPrefix}-{randomNumber}";
    }

    private static string CalculateEstimatedDuration(decimal distanceKm)
    {
        var totalMins = (int)Math.Ceiling((distanceKm / ApplicationConstants.Order.AverageSpeedKmPerHour) * 60);
        var hours = totalMins / 60;
        var minutes = totalMins % 60;
        return $"{hours}h {minutes}m";
    }

    private static decimal CalculatePrice(
        decimal distanceKm,
        decimal packageWeight,
        PackageSize packageSize,
        PricingOptions pricingOptions)
    {
        var sizeCharge = packageSize switch
        {
            PackageSize.Small => pricingOptions.SmallPackageCharge,
            PackageSize.Medium => pricingOptions.MediumPackageCharge,
            PackageSize.Large => pricingOptions.LargePackageCharge,
            PackageSize.XLarge => pricingOptions.XLargePackageCharge,
            _ => 0m
        };

        var totalPrice = (distanceKm * pricingOptions.PricePerKm)
                         + (pricingOptions.WeightMultiplier * packageWeight)
                         + sizeCharge;

        return Math.Round(Math.Max(totalPrice, pricingOptions.MinimumPrice), 2);
    }
}

