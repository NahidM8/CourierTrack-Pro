namespace CourierTrack.Application.Validators.Order;

public static class OrderStatusValidator
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> _allowedTransitions = new()
    {
        { OrderStatus.Created,    [OrderStatus.Pending, OrderStatus.Cancelled] },
        { OrderStatus.Pending,    [OrderStatus.Assigned, OrderStatus.Cancelled] },
        { OrderStatus.Assigned,   [OrderStatus.PickedUp, OrderStatus.Pending, OrderStatus.Cancelled] },
        { OrderStatus.PickedUp,   [OrderStatus.InTransit, OrderStatus.Cancelled] },
        { OrderStatus.InTransit,  [OrderStatus.Delivered, OrderStatus.Failed] },
        { OrderStatus.Delivered,  [] },
        { OrderStatus.Failed,     [] },
        { OrderStatus.Cancelled,  [] },
    };

    public static bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        return _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}
