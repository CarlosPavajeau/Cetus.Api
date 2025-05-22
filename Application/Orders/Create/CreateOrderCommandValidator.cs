using FluentValidation;

namespace Application.Orders.Create;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("La dirección es requerida.")
            .MaximumLength(256)
            .WithMessage("La dirección no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.CityId)
            .NotEmpty()
            .WithMessage("La ciudad es requerida.");

        RuleFor(x => x.Total)
            .NotEmpty()
            .WithMessage("El total es requerido.");

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
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El id del cliente es requerido.")
            .MaximumLength(50)
            .WithMessage("El id del cliente no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del cliente es requerido.")
            .MaximumLength(256)
            .WithMessage("El nombre del cliente no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email del cliente es requerido.")
            .MaximumLength(256)
            .WithMessage("El email del cliente no debe exceder los {MaxLength} caracteres.")
            .EmailAddress()
            .WithMessage("El email del cliente no es válido.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("El teléfono del cliente es requerido.")
            .MaximumLength(256)
            .WithMessage("El teléfono del cliente no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("La dirección del cliente es requerida.")
            .MaximumLength(256)
            .WithMessage("La dirección del cliente no debe exceder los {MaxLength} caracteres.");
    }
}

public sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("El nombre del producto es requerido.")
            .MaximumLength(256)
            .WithMessage("El nombre del producto no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(512)
            .WithMessage("La url de la imagen no debe exceder los {MaxLength} caracteres.");

        RuleFor(x => x.Quantity)
            .NotEmpty()
            .WithMessage("La cantidad es requerida.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El id del producto es requerido.");

        RuleFor(x => x.Price)
            .NotEmpty()
            .WithMessage("El precio es requerido.");
    }
}
