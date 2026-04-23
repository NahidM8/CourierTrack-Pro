namespace CourierTrack.Application.Mappers;

public class PaymentProfiler : Profile
{
    public PaymentProfiler()
    {
        CreateMap<Payment, PaymentDto>();
    }
}
