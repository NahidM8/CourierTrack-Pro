using CourierTrack.Domain.Exceptions;

namespace CourierTrack.Application.Services;

public class CourierRatingService : ICourierRatingService
{
    private readonly ICourierRatingRepository _courierRatingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly IMapper _mapper;

    public CourierRatingService(
        ICourierRatingRepository courierRatingRepository,
        IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        IMapper mapper)
    {
        _courierRatingRepository = courierRatingRepository;
        _orderRepository = orderRepository;
        _courierRepository = courierRepository;
        _mapper = mapper;
    }

    public async Task<CourierRatingDto> CreateRatingAsync(Guid orderId, RateCourierDto request, Guid customerId)
    {
        if (request.Rating is < 1 or > 5)
            throw new Domain.Exceptions.InvalidOperationException("Rating must be between 1 and 5.");

        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id {orderId} not found.");

        if (order.CustomerId != customerId)
            throw new Domain.Exceptions.InvalidOperationException("You can only rate your own order.");

        if (order.Status != OrderStatus.Delivered)
            throw new Domain.Exceptions.InvalidOperationException("Courier can only be rated after delivery.");

        if (!order.CourierId.HasValue)
            throw new Domain.Exceptions.InvalidOperationException("Order has no assigned courier.");

        var existingRating = await _courierRatingRepository.GetByOrderIdAsync(orderId);
        if (existingRating != null)
            throw new Domain.Exceptions.InvalidOperationException("This order has already been rated.");

        var courier = await _courierRepository.GetByIdAsync(order.CourierId.Value)
            ?? throw new NotFoundException($"Courier with id {order.CourierId.Value} not found.");

        var rating = new CourierRating
        {
            OrderId = orderId,
            CourierId = order.CourierId.Value,
            CustomerId = customerId,
            Score = request.Rating,
            Comment = request.Feedback,
            CreatedAt = DateTime.UtcNow
        };

        var createdRating = await _courierRatingRepository.AddAsync(rating);

        var ratingCount = courier.TotalDeliveries;
        var currentAverage = courier.Rating ?? 0m;
        var updatedAverage = ((currentAverage * ratingCount) + request.Rating) / (ratingCount + 1);

        courier.TotalDeliveries = ratingCount + 1;
        courier.Rating = Math.Round(updatedAverage, 2);

        await _courierRepository.UpdateAsync(courier);

        return _mapper.Map<CourierRatingDto>(createdRating);
    }

    public async Task<CourierRatingDto> GetRatingByOrderIdAsync(Guid orderId)
    {
        var rating = await _courierRatingRepository.GetByOrderIdAsync(orderId)
            ?? throw new NotFoundException($"Rating for order {orderId} not found.");

        return _mapper.Map<CourierRatingDto>(rating);
    }

    public async Task<IEnumerable<CourierRatingDto>> GetCourierRatingsAsync(Guid courierId)
    {
        var courier = await _courierRepository.GetByIdAsync(courierId)
            ?? throw new NotFoundException($"Courier with id {courierId} not found.");

        var ratings = await _courierRatingRepository.GetByCourierIdAsync(courierId);
        return _mapper.Map<IEnumerable<CourierRatingDto>>(ratings);
    }

    public async Task<IEnumerable<CourierRatingDto>> GetCustomerRatingsAsync(Guid customerId)
    {
        var ratings = await _courierRatingRepository.GetByCustomerIdAsync(customerId);
        return _mapper.Map<IEnumerable<CourierRatingDto>>(ratings);
    }

    public async Task<decimal?> GetCourierAverageRatingAsync(Guid courierId)
    {
        var courier = await _courierRepository.GetByIdAsync(courierId)
            ?? throw new NotFoundException($"Courier with id {courierId} not found.");

        return courier.Rating;
    }
}
