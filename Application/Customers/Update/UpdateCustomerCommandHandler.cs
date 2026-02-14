using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Update;

internal sealed class UpdateCustomerCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateCustomerCommand, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await db.Customers.FindAsync([command.Id], cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound(command.Id));
        }

        if (!string.Equals(customer.Phone, command.Phone, StringComparison.OrdinalIgnoreCase))
        {
            var phoneValidation = await EnsurePhoneIsUnique(command, cancellationToken);

            if (phoneValidation.IsFailure)
            {
                return phoneValidation;
            }
        }

        customer.Name = command.Name;
        customer.Phone = command.Phone;
        customer.Email = command.Email;
        customer.DocumentType = command.DocumentType;
        customer.DocumentNumber = command.DocumentNumber;
        customer.Address = command.Address;

        await db.SaveChangesAsync(cancellationToken);

        return new CustomerResponse(
            customer.Id,
            customer.DocumentType,
            customer.DocumentNumber,
            customer.Name,
            customer.Email,
            customer.Phone
        );
    }

    private async Task<Result<CustomerResponse>> EnsurePhoneIsUnique(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        bool phoneAlreadyUsed = await db.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Phone == command.Phone && c.Id != command.Id, cancellationToken);

        return phoneAlreadyUsed
            ? Result.Failure<CustomerResponse>(CustomerErrors.PhoneNumberAlreadyUsed(command.Phone))
            : Result.Success<CustomerResponse>(default!);
    }
}
