using SharedKernel;

namespace Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(Guid id)
    {
        return Error.NotFound(
            "Customers.NotFound",
            $"Customer with ID {id} was not found."
        );
    }

    public static Error NotFoundByPhone(string phone)
    {
        return Error.NotFound(
            "Customers.NotFoundByPhone",
            $"Customer with phone number {phone} was not found."
        );
    }
    
    public static Error PhoneNumberAlreadyUsed(string phone)
    {
        return Error.Conflict(
            "Customers.PhoneNumberAlreadyUsed",
            $"The phone number {phone} is already used by another customer."
        );
    }
}
