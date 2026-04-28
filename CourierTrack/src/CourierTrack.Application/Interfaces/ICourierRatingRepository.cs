namespace CourierTrack.Application.Interfaces;

public interface ICourierRatingRepository : IRepository<CourierRating>
{
    Task<CourierRating?> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<CourierRating>> GetByCourierIdAsync(Guid courierId);
    Task<IEnumerable<CourierRating>> GetByCustomerIdAsync(Guid customerId);
}
