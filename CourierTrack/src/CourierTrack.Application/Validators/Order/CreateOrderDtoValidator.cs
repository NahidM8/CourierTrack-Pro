namespace CourierTrack.Application.Validators.Order;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.PickupAddress)
            .NotEmpty()
            .WithMessage("Pickup address is required")
            .MaximumLength(500)
            .WithMessage("Pickup address must not exceed 500 characters");

        RuleFor(x => x.PickupLatitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Pickup latitude must be between -90 and 90");

        RuleFor(x => x.PickupLongitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Pickup longitude must be between -180 and 180");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty()
            .WithMessage("Delivery address is required")
            .MaximumLength(500)
            .WithMessage("Delivery address must not exceed 500 characters");

        RuleFor(x => x.DeliveryLatitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Delivery latitude must be between -90 and 90");

        RuleFor(x => x.DeliveryLongitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Delivery longitude must be between -180 and 180");

        RuleFor(x => x.PackageWeight)
            .GreaterThan(0)
            .WithMessage("Package weight must be greater than 0");

        RuleFor(x => x.PackageSize)
            .IsInEnum()
            .WithMessage("Invalid package size");

        RuleFor(x => x.PackageDescription)
            .MaximumLength(500)
            .WithMessage("Package description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.PackageDescription));
    }
}
