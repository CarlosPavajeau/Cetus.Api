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
}
