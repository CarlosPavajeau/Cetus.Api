using SharedKernel;

namespace Domain.Orders;

public static class OrderErrors
{
    public static Error InsufficientStock(List<string> OutOfStockProducts, List<string> RequestedProducts) =>
        Error.Problem(
            "Orders.InsufficientStock",
            $"Insufficient stock for products: {string.Join(", ", OutOfStockProducts)}. Requested quantities: {string.Join(", ", RequestedProducts)}"
        );

    public static Error ProductsNotFound(List<string> productIds) =>
        Error.NotFound(
            "Orders.ProductsNotFound",
            $"The following products were not found: {string.Join(", ", productIds)}"
        );

    public static Error NotFound(Guid orderId) =>
        Error.NotFound(
            "Orders.NotFound",
            $"Order with ID {orderId} was not found."
        );

    public static Error InvalidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus) =>
        Error.Problem(
            "Orders.InvalidStatusTransition",
            $"Cannot transition order from {currentStatus} to {newStatus}."
        );

    public static Error CustomerNotFound(string customerId) =>
        Error.NotFound(
            "Orders.CustomerNotFound",
            $"Customer with ID {customerId} was not found."
        );

    public static Error CreationFailed(string CustomerId, string message) =>
        Error.Problem(
            "Orders.CreationFailed",
            $"Failed to create order for customer {CustomerId}: {message}"
        );

    public static Error PaymentCreationFailed(Guid OrderId) =>
        Error.Problem(
            "Orders.PaymentCreationFailed",
            $"Failed to create payment for order {OrderId}."
        );

    public static Error EmptyOrder() =>
        Error.Problem("Orders.EmptyOrder", "Order must contain at least one item.");

    public static Error InvalidItemQuantities() =>
        Error.Problem("Orders.InvalidItemQuantities", "All item quantities must be positive.");

    public static Error AlreadyCanceled(Guid id) =>
        Error.Conflict("Orders.AlreadyCanceled", $"Order with ID {id} is already canceled.");

    public static Error InvalidTransactionId(Guid orderId) =>
        Error.Problem(
            "Orders.InvalidTransactionId",
            $"The transaction ID for order {orderId} is invalid."
        );

    public static Error PaymentNotFound(long paymentId) =>
        Error.NotFound(
            "Orders.PaymentNotFound",
            $"Payment with ID {paymentId} was not found."
        );

    public static Error PaymentCancellationFailed(long paymentId) =>
        Error.Problem(
            "Orders.PaymentCancellationFailed",
            $"Failed to cancel payment with ID {paymentId}."
        );

    public static Error NotPaid(Guid orderId) =>
        Error.Problem(
            "Orders.NotPaid",
            $"Order with ID {orderId} has not been paid."
        );

    public static Error PaymentSearchFailed(Guid orderId) =>
        Error.Problem(
            "Orders.PaymentSearchFailed",
            $"Failed to search payment for order with ID {orderId}."
        );

    public static Error ChangeStatusFailed(Guid orderId, string reason) =>
        Error.Problem(
            "Orders.ChangeStatusFailed",
            $"Failed to change status for order with ID {orderId}. Reason: {reason}"
        );
}
