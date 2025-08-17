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

        RuleFor(x => x.Price)
            .NotNull()
            .NotEmpty()
            .WithMessage("El precio del producto es requerido.")
            .GreaterThan(0)
            .WithMessage("El precio del producto debe ser mayor a 0.");

        RuleFor(x => x.Stock)
            .NotNull()
            .NotEmpty()
            .WithMessage("La cantidad en stock del producto es requerida.")
            .GreaterThan(0)
            .WithMessage("La cantidad en stock del producto debe ser mayor a 0.");
        
        RuleFor(x => x.CategoryId)
            .NotNull()
            .NotEmpty()
            .WithMessage("El identificador de la categoría del producto es requerido.");

        RuleFor(x => x.Images)
            .NotNull()
            .NotEmpty()
            .WithMessage("Las imágenes del producto son requeridas");
    }
}
