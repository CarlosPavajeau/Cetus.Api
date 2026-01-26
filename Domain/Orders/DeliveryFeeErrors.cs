using SharedKernel;

namespace Domain.Orders;

public static class DeliveryFeeErrors
{
    public static Error AlreadyExists(Guid cityId) =>
        Error.Conflict(
            "DeliveryFees.AlreadyExists",
            $"A delivery fee for city with ID {cityId} already exists."
        );
}
