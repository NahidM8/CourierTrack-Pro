using Microsoft.Extensions.Options;

namespace CourierTrack.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly PricingOptions _pricingOptions;

    public OrderService(IOrderRepository orderRepository, IMapper mapper, IOptions<PricingOptions> pricingOptions)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _pricingOptions = pricingOptions.Value;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order is null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetByTrackingNumberAsync(string trackingNumber)
    {
        var order = await _orderRepository.GetByTrackingNumberAsync(trackingNumber);
        return order is null ? null : _mapper.Map<OrderDto>(order);
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
        var estimatedDistanceKm = CalculateDistanceKm(
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

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderDto request)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException($"Order with id {orderId} not found.");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cancelled orders cannot be updated.");

        order.Status = request.Status;

        if (order.Status == OrderStatus.PickedUp)
            order.PickedUpAt ??= DateTime.UtcNow;

        if (order.Status == OrderStatus.Delivered)
            order.DeliveredAt ??= DateTime.UtcNow;

        var updatedOrder = await _orderRepository.UpdateAsync(order);
        return _mapper.Map<OrderDto>(updatedOrder);
    }

    public async Task<OrderDto> CancelOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException($"Order with id {orderId} not found.");

        if (order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Delivered orders cannot be cancelled.");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        order.Status = OrderStatus.Cancelled;

        var updatedOrder = await _orderRepository.UpdateAsync(order);
        return _mapper.Map<OrderDto>(updatedOrder);
    }

    private static string GenerateTrackingNumber()
    {
        var randomNumber = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"CT-{randomNumber}";
    }

    private static decimal CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round((decimal)(earthRadiusKm * c), 2);
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private static string CalculateEstimatedDuration(decimal distanceKm)
    {
        const decimal averageSpeedKmPerHour = 35m;
        var totalMinutes = Math.Ceiling((distanceKm / averageSpeedKmPerHour) * 60);
        var hours = (int)totalMinutes / 60;
        var minutes = (int)totalMinutes % 60;
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

