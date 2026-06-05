namespace CourierTrack.Application.DTOs;

public class CourierFilterDto : PagedQuery
{
    public bool? IsAvailable { get; set; }
    public VehicleType? VehicleType { get; set; }
}
