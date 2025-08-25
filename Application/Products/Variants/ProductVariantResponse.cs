namespace Application.Products.Variants;

public sealed record ProductVariantResponse(
    long Id,
    string Sku,
    decimal Price,
    int StockQuantity,
    IEnumerable<ProductImageResponse> Images,
    IEnumerable<VariantOptionValueResponse> OptionValues
);
