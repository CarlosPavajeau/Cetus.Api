using FluentValidation;

namespace Application.Products.Options.Create;

public sealed class CreateProductOptionCommandValidator : AbstractValidator<CreateProductOptionCommand>
{
    public CreateProductOptionCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El id del producto es requerido");

        RuleFor(x => x.OptionTypeId)
            .GreaterThan(0)
            .WithMessage("La opción es requerida");
    }
}
