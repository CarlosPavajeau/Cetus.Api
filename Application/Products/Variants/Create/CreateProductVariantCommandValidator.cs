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
            .NotEmpty()
            .WithMessage("Sku is required.")
            .MaximumLength(100)
            .WithMessage("Sku must not exceed 100 characters.");

        RuleFor(p => p.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(p => p.StockQuantity)
            .GreaterThan(0)
            .WithMessage("Stock quantity must be greater than 0.");

        RuleFor(p => p.Images)
            .NotEmpty()
            .WithMessage("Images are required.");
    }
}
