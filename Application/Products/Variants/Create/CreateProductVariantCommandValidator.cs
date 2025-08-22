using FluentValidation;

namespace Application.Products.Variants.Create;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(p => p.ProductId)
            .NotEmpty()
            .WithMessage("Product Id is required.");

        RuleFor(p => p.Sku)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Sku is required.")
            .MaximumLength(100).WithMessage("Sku must not exceed 100 characters.")
            .Matches("^[A-Za-z0-9_-]$").WithMessage("Sku may contain letters, numbers, '-' and '_' only.");

        RuleFor(p => p.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity must be greater than or equal to 0.");

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
