namespace CourierTrack.Application.Mappers;

public class CourierProfiler : Profile
{
    public CourierProfiler()
    {
        CreateMap<Courier, CourierDto>()
            .ConstructUsing(src => new CourierDto(
                src.Id,
                src.UserId,
                src.User.FullName,
                src.User.PhoneNumber,
                src.VehicleType,
                src.CurrentLatitude,
                src.CurrentLongitude,
                src.IsAvailable,
                src.Rating,
                src.TotalDeliveries
            ));
    }
}
