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
            .NotEmpty()
            .WithMessage("{PropertyName} must not be empty")
            .MaximumLength(100)
            .WithMessage("{PropertyName} must not exceed 100 characters");

        RuleFor(x => x.Adjustments)
            .NotEmpty()
            .WithMessage("{PropertyName} must not be empty");

        RuleForEach(x => x.Adjustments)
            .ChildRules(i =>
            {
                i.RuleFor(p => p.VariantId)
                    .GreaterThan(0)
                    .WithMessage("{PropertyName} must be greater than 0");

                i.RuleFor(p => p.Value)
                    .NotEqual(0)
                    .WithMessage("{PropertyName} must not be equal to 0");

                i.RuleFor(p => p.Reason)
                    .MaximumLength(100)
                    .WithMessage("{PropertyName} must not exceed 100 characters")
                    .Must((_, reason, context) =>
                    {
                        if (!context.RootContextData.TryGetValue("GlobalReason", out object? globalReasonObj) ||
                            globalReasonObj is not string globalReason)
                        {
                            return (reason?.Length ?? 0) <= 255;
                        }

                        int combinedLength = globalReason.Length + (reason?.Length ?? 0) + 3;
                        return combinedLength <= 255;
                    })
                    .WithMessage("The combined length of GlobalReason and Reason must not exceed 255 characters");
            });
    }
}
