using CourierTrack.Application.DTOs;
using CourierTrack.Domain.Enums;
using FluentValidation;

namespace CourierTrack.Application.Validators.Order;

public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderDtoValidator()
    {
        RuleFor(x => x.CourierId)
            .NotEmpty()
            .WithMessage("Courier ID is required when assigning an order")
            .When(x => x.Status == OrderStatus.Assigned);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status");

        RuleFor(x => x.PickedUpAt)
            .NotEmpty()
            .WithMessage("Pickup time is required when order is picked up")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Pickup time cannot be in the future")
            .When(x => x.Status == OrderStatus.PickedUp || x.Status == OrderStatus.InTransit || x.Status == OrderStatus.Delivered);

        RuleFor(x => x.DeliveredAt)
            .NotEmpty()
            .WithMessage("Delivery time is required when order is delivered")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Delivery time cannot be in the future")
            .GreaterThanOrEqualTo(x => x.PickedUpAt)
            .WithMessage("Delivery time must be after pickup time")
            .When(x => x.Status == OrderStatus.Delivered);

        RuleFor(x => x.DeliveredAt)
            .Null()
            .WithMessage("Delivery time must be null for non-delivered orders")
            .When(x => x.Status != OrderStatus.Delivered);

        RuleFor(x => x.PickedUpAt)
            .Null()
            .WithMessage("Pickup time must be null for cancelled orders")
            .When(x => x.Status == OrderStatus.Cancelled);

        RuleFor(x => x.DeliveredAt)
            .Null()
            .WithMessage("Delivery time must be null for cancelled orders")
            .When(x => x.Status == OrderStatus.Cancelled);
    }
}
