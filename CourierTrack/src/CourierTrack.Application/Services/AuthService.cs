namespace CourierTrack.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        ICourierRepository courierRepository,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _courierRepository = courierRepository;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new Domain.Exceptions.InvalidOperationException("User with this email already exists.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed: {Errors}", errors);
            throw new Domain.Exceptions.InvalidOperationException($"User creation failed: {errors}");
        }

        // Create Courier entity if user is registering as Courier
        if (request.Role == Role.Courier && request.VehicleType.HasValue)
        {
            try
            {
                var courier = new Courier
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VehicleType = request.VehicleType.Value,
                    IsAvailable = true,
                    TotalDeliveries = 0
                };
                await _courierRepository.AddAsync(courier);
                _logger.LogInformation("Courier created successfully for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating courier for user {UserId}", user.Id);
                throw;
            }
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken is null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found or inactive.");

        refreshToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        string resetToken = string.Empty;

        if (user is not null)
        {
            resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            Console.WriteLine($"[DEV ONLY] Password reset token for {user.Email}: {resetToken}");
        }

        return new ForgotPasswordResponseDto("If an account exists with this email, a password reset link has been sent.");
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new Domain.Exceptions.InvalidOperationException("User not found.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Domain.Exceptions.InvalidOperationException($"Password change failed: {errors}");
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        var roles = new List<string> { user.Role.ToString() };
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResponseDto(
            accessToken,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }
}
