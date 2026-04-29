using CourierTrack.Domain.Common;

namespace CourierTrack.Application.DTOs;

public class OrderFilterDto : PagedQuery
{
    public Guid? CustomerId { get; set; }
    public Guid? CourierId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
