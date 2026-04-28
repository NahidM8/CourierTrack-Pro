namespace CourierTrack.Application.Interfaces;

public interface ICourierRatingService
{
    Task<CourierRatingDto> CreateRatingAsync(Guid orderId, RateCourierDto request, Guid customerId);
    Task<CourierRatingDto> GetRatingByOrderIdAsync(Guid orderId);
    Task<IEnumerable<CourierRatingDto>> GetCourierRatingsAsync(Guid courierId);
    Task<IEnumerable<CourierRatingDto>> GetCustomerRatingsAsync(Guid customerId);
    Task<decimal?> GetCourierAverageRatingAsync(Guid courierId);
}
