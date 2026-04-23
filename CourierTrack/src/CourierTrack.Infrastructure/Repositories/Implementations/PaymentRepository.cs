namespace CourierTrack.Infrastructure.Repositories.Implementations;

public class PaymentRepository(CourierTrackDbContext _context) : Repository<Payment>(_context), IPaymentRepository
{
    public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);
    }
}