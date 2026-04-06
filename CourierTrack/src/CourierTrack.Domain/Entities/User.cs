using CourierTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CourierTrack.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Courier? Courier { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
