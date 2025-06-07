using Domain.Products;

namespace Application.Products.TopSelling;

public sealed record TopSellingProductResponse(Guid Id, string Name, string? ImageUrl, int SalesCount, string? Category)
{
    public static TopSellingProductResponse FromProduct(Product product)
    {
        return new TopSellingProductResponse(
            product.Id,
            product.Name,
            product.ImageUrl,
            product.SalesCount,
            product.Category?.Name);
    }
}
