using FluentValidation;

namespace Application.Products.Create;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("El nombre del producto es requerido.");

        RuleFor(x => x.CategoryId)
            .NotNull()
            .NotEmpty()
            .WithMessage("El identificador de la categoría del producto es requerido.");
    }
}
