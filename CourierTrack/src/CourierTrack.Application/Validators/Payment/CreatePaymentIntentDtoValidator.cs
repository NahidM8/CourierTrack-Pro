namespace CourierTrack.Application.Validators.Payment;

public class CreatePaymentIntentDtoValidator : AbstractValidator<CreatePaymentIntentDto>
{
    public CreatePaymentIntentDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Invalid payment method");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency code must be 3 characters (ISO 4217)")
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Currency code must be 3 uppercase letters");
    }
}
