using SharedKernel;

namespace Domain.Products;

public static class ProductVariantErrors
{
    public static Error MissingOptionValues() => Error.Problem(
        "Product.Variant.MissingOptionValues",
        "Some option values do not exist."
    );

    public static Error UnexpectedError() => Error.Problem(
        "Product.Variant.Create",
        "An unexpected error occurred while creating the product variant."
    );
}
