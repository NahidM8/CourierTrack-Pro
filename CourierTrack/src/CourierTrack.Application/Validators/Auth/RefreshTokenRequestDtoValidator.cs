using CourierTrack.Application.DTOs;
using FluentValidation;

namespace CourierTrack.Application.Validators.Auth;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
