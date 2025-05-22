using SharedKernel;

namespace Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(string productId) =>
        Error.NotFound(
            "Products.NotFound",
            $"Product with ID {productId} was not found."
        );
}
