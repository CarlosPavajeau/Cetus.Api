namespace Application.Products.Variants;

public sealed record ProductVariantResponse(
    long Id,
    string Sku,
    decimal Price,
    int Stock,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<VariantOptionValueResponse> OptionValues
);
