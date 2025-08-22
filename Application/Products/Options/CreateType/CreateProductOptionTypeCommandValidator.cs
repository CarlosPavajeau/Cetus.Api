using FluentValidation;

namespace Application.Products.Options.CreateType;

public sealed class CreateProductOptionTypeCommandValidator : AbstractValidator<CreateProductOptionTypeCommand>
{
    private const int NameMaxLength = 100;
    private const int ValueMaxLength = 50;
    private const int MaxValuesPerType = 50;

    public CreateProductOptionTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(NameMaxLength);

        RuleFor(x => x.Values)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Values list is required.")
            .NotEmpty().WithMessage("At least one value is required.")
            .Must(values => values.Count <= MaxValuesPerType)
            .WithMessage($"A maximum of {MaxValuesPerType} values are allowed.")
            // Enforce uniqueness (case-insensitive, ignoring surrounding whitespace)
            .Must(values =>
                values.Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v.ToUpperInvariant())
                    .Distinct().Count() == values.Count)
            .WithMessage("Values must be unique (case-insensitive).");

        RuleForEach(x => x.Values)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Each value must be non-empty.")
            .MaximumLength(ValueMaxLength);
    }
}
