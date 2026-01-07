using FluentValidation;

namespace Application.Orders.ChangeStatus;

public sealed class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(c => c.OrderId)
            .NotEmpty()
            .WithMessage("Order id is required");

        RuleFor(c => c.NewStatus)
            .IsInEnum()
            .WithMessage("New status is invalid");
    }
}
