using SharedKernel;

namespace Domain.PaymentLinks;

public static class PaymentLinkErrors
{
    public static Error AlreadyPaid(Guid orderId) =>
        Error.Conflict(
            "PaymentLinks.AlreadyPaid",
            $"Order with ID {orderId} has already been paid."
        );

    public static Error ActiveLinkExists(Guid orderId) =>
        Error.Conflict(
            "PaymentLinks.ActiveLinkExists",
            $"An active payment link already exists for order with ID {orderId}."
        );

    public static Error NotFound(string token) =>
        Error.NotFound(
            "PaymentLinks.NotFoundByToken",
            $"No payment link found for token {token}."
        );
}
