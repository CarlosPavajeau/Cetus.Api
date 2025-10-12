using Domain.Products;

namespace Application.Products.Variants;

public sealed record SimpleProductVariantResponse(long Id, string Sku, int Stock, decimal Price, Guid ProductId)
{
    public static SimpleProductVariantResponse From(ProductVariant variant) =>
        new(variant.Id, variant.Sku, variant.StockQuantity, variant.Price, variant.ProductId);
}
