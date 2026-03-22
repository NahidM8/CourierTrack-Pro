namespace CourierTrack.Domain.Common;

public class AuditableEntity() : BaseEntity
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; private set; }
}
