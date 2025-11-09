using FluentValidation;

namespace Application.Products.Variants.Images.Add;

public sealed class AddVariantImagesCommandValidator : AbstractValidator<AddVariantImagesCommand>
{
    public AddVariantImagesCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage("The id cannot be empty");

        RuleFor(p => p.Images)
            .NotNull()
            .WithMessage("Images collection must not be null.");

        RuleForEach(p => p.Images)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                    .NotEmpty().WithMessage("ImageUrl is required.");
                img.RuleFor(i => i.SortOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("SortOrder must be >= 0.");
                img.RuleFor(i => i.AltText)
                    .MaximumLength(200).When(i => !string.IsNullOrWhiteSpace(i.AltText));
            });
    }
}
