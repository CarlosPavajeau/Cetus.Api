using SharedKernel;

namespace Domain.Products;

public static class ProductOptionTypeErrors
{
    public static Error NotFound(long Id) => Error.NotFound(
        "ProductOptionType.NotFound",
        $"Product option type with ID '{Id}' was not found."
    );
}
