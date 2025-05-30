using SharedKernel;

namespace Domain.Orders;

public static class CustomerErrors
{
    public static Error NotFound(string customerId) =>
        Error.NotFound(
            "Customers.NotFound",
            $"Customer with ID {customerId} was not found."
        );
}
