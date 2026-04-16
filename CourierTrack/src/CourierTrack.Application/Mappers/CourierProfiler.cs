namespace CourierTrack.Application.Mappers;

public class CourierProfiler : Profile
{
    public CourierProfiler()
    {
        CreateMap<Courier, CourierDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
    }
}
