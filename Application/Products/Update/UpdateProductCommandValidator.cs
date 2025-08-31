using FluentValidation;

namespace Application.Products.Update;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty()
            .WithMessage("El identificador del producto es requerido.");

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("El nombre del producto es requerido.");
    }
}
