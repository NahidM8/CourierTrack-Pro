namespace CourierTrack.API.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var response = await authService.RegisterAsync(request);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenRequestDto request)
    {
        var response = await authService.RefreshTokenAsync(request);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(ForgotPasswordRequestDto request)
    {
        var response = await authService.ForgotPasswordAsync(request);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user context." });
        }

        await authService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Password changed successfully." });
    }
}
