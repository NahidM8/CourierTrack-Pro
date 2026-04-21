namespace CourierTrack.Application.DTOs;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    Role Role,
    DateTime CreatedAt,
    bool IsActive
);
