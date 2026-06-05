namespace CourierTrack.Application.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId);
}