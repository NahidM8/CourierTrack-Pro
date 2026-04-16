namespace CourierTrack.Domain.Enums;

public enum OrderStatus
{
    Created,
    Pending,
    Assigned,
    PickedUp,
    InTransit,
    Delivered,
    Failed,
    Cancelled
}