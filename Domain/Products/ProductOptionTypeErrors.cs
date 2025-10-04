using SharedKernel;

namespace Domain.Products;

public static class ProductOptionTypeErrors
{
    public static Error NotFound(long id) => Error.NotFound(
        "ProductOptionType.NotFound",
        $"Product option type with ID '{id}' was not found."
    );
}
