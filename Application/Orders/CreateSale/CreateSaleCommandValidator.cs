using FluentValidation;

namespace Application.Orders.CreateSale;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Debe agregar al menos un producto.");

        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemValidator());
        RuleFor(x => x.Customer).SetValidator(new CreateSaleCustomerValidator());
    }
}

public sealed class CreateSaleItemValidator : AbstractValidator<CreateSaleItem>
{
    public CreateSaleItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("La cantidad es requerida.");

        RuleFor(x => x.VariantId)
            .GreaterThan(0)
            .WithMessage("El id de la variante es requerido.");
    }
}

public sealed class CreateSaleCustomerValidator : AbstractValidator<CreateSaleCustomer>
{
    public CreateSaleCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del cliente es requerido.")
            .MaximumLength(256)
            .WithMessage("El nombre del cliente no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email del cliente no es válido.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("El teléfono del cliente es requerido.")
            .MaximumLength(256)
            .WithMessage("El teléfono del cliente no debe exceder los {MaxLength} caracteres.");
    }
}
