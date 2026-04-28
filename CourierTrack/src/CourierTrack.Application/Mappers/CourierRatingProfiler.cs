namespace CourierTrack.Application.Mappers;

public class CourierRatingProfiler : Profile
{
    public CourierRatingProfiler()
    {
        CreateMap<CourierRating, CourierRatingDto>();
    }
}
