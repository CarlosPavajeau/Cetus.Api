using FluentValidation;

namespace Application.Products.CreateSimple;

public sealed class CreateSimpleProductCommandValidator : AbstractValidator<CreateSimpleProductCommand>
{
    public CreateSimpleProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("El nombre del producto es requerido.");

        RuleFor(x => x.CategoryId)
            .NotNull()
            .NotEmpty()
            .WithMessage("El identificador de la categoría del producto es requerido.");

        RuleFor(p => p.Sku)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El sku es requerido")
            .MaximumLength(100).WithMessage("El SKU no debe exceder los 100 caracteres")
            .Matches("^[a-zA-Z0-9-]+$").WithMessage("El SKU puede contener solo letras, números y '-'");

        RuleFor(p => p.Price)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor que 0");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La cantidad de stock debe ser mayor o igual a 0");

        RuleFor(p => p.Images)
            .NotNull()
            .WithMessage("La colección de imágenes no debe ser nula");

        RuleForEach(p => p.Images)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                    .NotEmpty().WithMessage("Se requiere ImageUrl");
                img.RuleFor(i => i.SortOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("El orden de clasificación debe ser >= 0");
                img.RuleFor(i => i.AltText)
                    .MaximumLength(200).When(i => !string.IsNullOrWhiteSpace(i.AltText));
            });
    }
}
