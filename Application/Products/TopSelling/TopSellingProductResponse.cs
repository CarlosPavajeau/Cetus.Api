using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.TopSelling;

public sealed record TopSellingProductResponse(Guid Id, string Name, string? ImageUrl, int SalesCount, string? Category)
{
    public static Expression<Func<ProductVariant, TopSellingProductResponse>> Map => variant =>
        new TopSellingProductResponse(
            variant.ProductId,
            variant.Product!.Name,
            variant.Images.OrderBy(i => i.SortOrder).FirstOrDefault()!.ImageUrl,
            variant.SalesCount,
            variant.Product!.Category!.Name);
}
