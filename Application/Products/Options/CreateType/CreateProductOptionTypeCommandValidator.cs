using FluentValidation;

namespace Application.Products.Options.CreateType;

public sealed class CreateProductOptionTypeCommandValidator : AbstractValidator<CreateProductOptionTypeCommand>
{
    public CreateProductOptionTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Values)
            .NotNull().WithMessage("Values list is required.")
            .NotEmpty().WithMessage("At least one value is required.")
            .ForEach(value => value
                .NotEmpty().WithMessage("Each value must be non-empty.")
                .MaximumLength(50));
    }
}
