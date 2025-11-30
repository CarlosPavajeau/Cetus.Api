using FluentValidation;

namespace Application.Products.Inventory.Adjust;

public sealed class AdjustInventoryStockCommandValidator : AbstractValidator<AdjustInventoryStockCommand>
{
    public AdjustInventoryStockCommandValidator()
    {
        RuleFor(x => x.GlobalReason)
            .MaximumLength(100)
            .WithMessage("{PropertyName} must not exceed 100 characters");

        RuleFor(x => x.UserId)
            .MaximumLength(100)
            .WithMessage("{PropertyName} must not exceed 100 characters");

        RuleFor(x => x.Adjustments)
            .NotEmpty()
            .WithMessage("{PropertyName} must not be empty.");

        RuleForEach(x => x.Adjustments)
            .ChildRules(i =>
            {
                i.RuleFor(p => p.VariantId)
                    .NotEmpty()
                    .WithMessage("{PropertyName} must not be empty.")
                    .GreaterThan(0)
                    .WithMessage("{PropertyName} must be greater than 0");

                i.RuleFor(p => p.Value)
                    .NotEqual(0)
                    .WithMessage("{PropertyName} must be not equal to 0");

                i.RuleFor(p => p.Reason)
                    .MaximumLength(100)
                    .WithMessage("{PropertyName} must not exceed 100 characters");
            });
    }
}
