namespace CourierTrack.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task UpdateStatusAsync(Guid id, bool isActive);
}
