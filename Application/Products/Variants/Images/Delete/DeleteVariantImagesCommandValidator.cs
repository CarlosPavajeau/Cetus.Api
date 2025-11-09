using FluentValidation;

namespace Application.Products.Variants.Images.Delete;

public sealed class DeleteVariantImagesCommandValidator : AbstractValidator<DeleteVariantImageCommand>
{
    public DeleteVariantImagesCommandValidator()
    {
        RuleFor(c => c.VariantId)
            .NotEmpty()
            .WithMessage("The VariantId cannot be empty");

        RuleFor(c => c.ImageId)
            .NotEmpty()
            .WithMessage("The ImageId cannot be empty");
    }
}
