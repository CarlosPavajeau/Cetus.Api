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

    public static Error OptionValuesCrossStore() => Error.Problem(
        "Product.Variant.OptionValuesCrossStore",
        "Option values do not belong to the product's store."
    );

    public static Error OptionTypesNotAttached() => Error.Problem(
        "Product.Variant.OptionTypesNotAttached",
        "Option values must belong to option types attached to the product."
    );

    public static Error DuplicateSku(string sku) => Error.Conflict(
        "Product.Variant.DuplicateSku",
        $"A variant with SKU '{sku}' already exists for this product."
    );

    public static Error DuplicateCombination() => Error.Conflict(
        "Product.Variant.DuplicateCombination",
        "A variant with the same option value combination already exists for this product."
    );
}
