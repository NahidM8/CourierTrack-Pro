namespace CourierTrack.Application.Validators.Payment;

public class RefundPaymentDtoValidator : AbstractValidator<RefundPaymentDto>
{
    public RefundPaymentDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Refund reason is required")
            .MinimumLength(5)
            .WithMessage("Refund reason must be at least 5 characters")
            .MaximumLength(500)
            .WithMessage("Refund reason must not exceed 500 characters");
    }
}
