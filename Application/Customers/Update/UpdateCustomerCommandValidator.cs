using FluentValidation;

namespace Application.Customers.Update;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage("El id del cliente es requerido.");

        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("El nombre del cliente es requerido.")
            .MaximumLength(256)
            .WithMessage("El nombre del cliente no puede exceder los 256 caracteres.");

        RuleFor(c => c.Email)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("El email del cliente no es válido.");

        RuleFor(c => c.Phone)
            .NotEmpty()
            .WithMessage("El teléfono del cliente es requerido.")
            .MaximumLength(20)
            .WithMessage("El teléfono del cliente no debe exceder los {MaxLength} caracteres.");

        RuleFor(c => c.Address)
            .MaximumLength(256)
            .When(c => !string.IsNullOrWhiteSpace(c.Address))
            .WithMessage("La dirección del cliente no debe exceder los {MaxLength} caracteres.");

        RuleFor(c => c.DocumentNumber)
            .MaximumLength(20)
            .When(c => !string.IsNullOrWhiteSpace(c.DocumentNumber))
            .WithMessage("El número de documento del cliente no debe exceder los {MaxLength} caracteres.");
    }
}
