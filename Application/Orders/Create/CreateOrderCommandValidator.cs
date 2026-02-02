using FluentValidation;

namespace Application.Orders.Create;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Shipping.Address)
            .NotEmpty()
            .WithMessage("La dirección es requerida.")
            .MaximumLength(256)
            .WithMessage("La dirección no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.Shipping.CityId)
            .NotEmpty()
            .WithMessage("La ciudad es requerida.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Debe agregar al menos un producto.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());

        RuleFor(x => x.Customer).SetValidator(new CreateOrderCustomerValidator());
    }
}

public sealed class CreateOrderCustomerValidator : AbstractValidator<CreateOrderCustomer>
{
    public CreateOrderCustomerValidator()
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

public sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("La cantidad es requerida.");

        RuleFor(x => x.VariantId)
            .GreaterThan(0)
            .WithMessage("El id de la variante es requerido.");
    }
}
