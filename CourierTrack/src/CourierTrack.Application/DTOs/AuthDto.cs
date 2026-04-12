namespace CourierTrack.Application.DTOs;

public record RegisterRequestDto(
    string FullName,
    string Email,
    string Password,
    string PhoneNumber,
    Role Role
    );

public record LoginRequestDto(
    string Email,
    string Password
    );

public record RefreshTokenRequestDto(
    string RefreshToken
    );

public record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
    );

public record ForgotPasswordRequestDto(
    string Email
    );

public record ForgotPasswordResponseDto(
    string Message
    );

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
    );
