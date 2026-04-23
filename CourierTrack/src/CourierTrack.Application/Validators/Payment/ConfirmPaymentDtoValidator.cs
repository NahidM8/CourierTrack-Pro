namespace CourierTrack.Application.Validators.Payment;

public class ConfirmPaymentDtoValidator : AbstractValidator<ConfirmPaymentDto>
{
    public ConfirmPaymentDtoValidator()
    {
        RuleFor(x => x.StripePaymentIntentId)
            .NotEmpty()
            .WithMessage("Stripe payment intent ID is required")
            .MinimumLength(10)
            .WithMessage("Stripe payment intent ID is invalid");
    }
}
