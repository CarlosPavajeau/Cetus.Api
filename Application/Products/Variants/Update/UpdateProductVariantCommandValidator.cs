using FluentValidation;

namespace Application.Products.Variants.Update;

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.RetailPrice)
            .GreaterThan(0)
            .When(p => p.RetailPrice is not null)
            .WithMessage("RetailPrice must be greater than 0.");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(0)
            .When(p => p.CompareAtPrice is not null)
            .WithMessage("CompareAtPrice must be greater than 0.")
            .GreaterThanOrEqualTo(x => x.Price)
            .When(p => p.CompareAtPrice is not null)
            .WithMessage("CompareAtPrice must be greater than or equal to Price.");
    }
}
