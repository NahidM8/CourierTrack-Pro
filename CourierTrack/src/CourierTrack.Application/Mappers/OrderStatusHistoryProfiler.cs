namespace CourierTrack.Application.Mappers;

public class OrderStatusHistoryProfiler : Profile
{
    public OrderStatusHistoryProfiler()
    {
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();
    }
}
