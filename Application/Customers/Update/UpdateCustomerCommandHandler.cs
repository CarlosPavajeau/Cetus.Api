using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Customers.Update;

internal sealed class UpdateCustomerCommandHandler(IApplicationDbContext db, HybridCache cache)
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

        string normalizedPhone = new([.. command.Phone.Where(char.IsDigit)]);
        string oldPhone = customer.Phone;

        customer.Name = command.Name;
        customer.Phone = normalizedPhone;
        customer.Email = command.Email;
        customer.DocumentType = command.DocumentType;
        customer.DocumentNumber = command.DocumentNumber;
        customer.Address = command.Address;

        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync($"customer-by-phone-{oldPhone}", cancellationToken);

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
