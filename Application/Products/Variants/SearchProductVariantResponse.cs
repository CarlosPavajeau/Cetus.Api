namespace Application.Products.Variants;

public record SearchProductVariantResponse(
    long Id,
    string Sku,
    decimal Price,
    int Stock,
    string? ImageUrl,
    IReadOnlyList<VariantOptionValueResponse> OptionValues
);
