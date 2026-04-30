namespace CourierTrack.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetAllPagedAsync(OrderFilterDto filter);
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto> GetByIdAsync(Guid id);
    Task<OrderDto> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<OrderDto>> GetByCourierIdAsync(Guid courierId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto request);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderDto request, Guid changedBy);
    Task<OrderDto> CancelOrderAsync(Guid orderId, Guid changedBy);
    Task<IEnumerable<OrderStatusHistoryDto>> GetStatusHistoryAsync(Guid orderId);
    Task<OrderDto> RateCourierAsync(Guid orderId, RateCourierDto request, Guid customerId);
}
