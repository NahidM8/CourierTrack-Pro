using CourierTrack.Domain.Enums;

namespace CourierTrack.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    //public Courier? Courier { get; set; }
}
