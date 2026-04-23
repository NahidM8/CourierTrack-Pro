namespace CourierTrack.Application.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId);
}