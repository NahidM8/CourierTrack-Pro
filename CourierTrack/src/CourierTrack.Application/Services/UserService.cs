using CourierTrack.Domain.Exceptions;

namespace CourierTrack.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with id {id} not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new NotFoundException($"User with email {email} not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task UpdateStatusAsync(Guid id, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with id {id} not found.");

        user.IsActive = isActive;
        await _userRepository.UpdateAsync(user);
    }
}
