using SharedKernel;

namespace Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(string productId) =>
        Error.NotFound(
            "Products.NotFound",
            $"Product with ID {productId} was not found."
        );

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound(
            "Products.NotFoundBySlug",
            $"Product with slug '{slug}' was not found."
        );

    public static Error VariantNotFound(long id) =>
        Error.NotFound(
            "Products.VariantNotFound",
            $"Product variant with ID {id} was not found."
        );
}
