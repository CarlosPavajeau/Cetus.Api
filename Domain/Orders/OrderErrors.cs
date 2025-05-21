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
}
