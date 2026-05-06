namespace CourierTrack.Domain.Entities;

public class RefreshToken
{
    public int Id { get; init; }
    public Guid UserId { get; init; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;

    public User User { get; set; } = null!;
}
