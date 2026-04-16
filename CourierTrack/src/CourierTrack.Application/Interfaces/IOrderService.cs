namespace CourierTrack.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto> GetByIdAsync(Guid id);
    Task<OrderDto> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<OrderDto>> GetByCourierIdAsync(Guid courierId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto request);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderDto request);
    Task<OrderDto> CancelOrderAsync(Guid orderId);
}
