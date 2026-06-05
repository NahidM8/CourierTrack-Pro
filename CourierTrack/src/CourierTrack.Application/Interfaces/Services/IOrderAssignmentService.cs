namespace CourierTrack.Application.Interfaces.Services;

public interface IOrderAssignmentService
{
    Task<OrderDto> AssignOrderAsync(Guid orderId, Guid courierId);
    Task<OrderDto> AutoAssignOrderAsync(Guid orderId);
    Task<OrderDto> UnassignOrderAsync(Guid orderId);
    Task<IEnumerable<OrderDto>> GetUnassignedOrdersAsync();
    Task<bool> IsOrderAssignedAsync(Guid orderId);
}
