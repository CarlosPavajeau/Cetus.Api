using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products;

public sealed record SimpleProductForSaleResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ImageUrl,
    Guid CategoryId,
    decimal Price,
    decimal Rating,
    int ReviewsCount,
    long VariantId)
{
    public static Expression<Func<ProductVariant, SimpleProductForSaleResponse>> Map => variant =>
        new SimpleProductForSaleResponse(
            variant.ProductId,
            variant.Product!.Name,
            variant.Product!.Slug,
            variant.Images.OrderBy(i => i.SortOrder).FirstOrDefault()!.ImageUrl,
            variant.Product!.CategoryId,
            variant.Price,
            variant.Product!.Rating,
            variant.Product!.ReviewsCount,
            variant.Id
        );
}
