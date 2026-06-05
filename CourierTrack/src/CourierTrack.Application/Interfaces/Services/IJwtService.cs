namespace CourierTrack.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    RefreshToken GenerateRefreshToken(Guid userId);
}
