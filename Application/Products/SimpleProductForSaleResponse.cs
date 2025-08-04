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
    int ReviewsCount)
{
    public static Expression<Func<Product, SimpleProductForSaleResponse>> Map => product =>
        new SimpleProductForSaleResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Images.FirstOrDefault()!.ImageUrl,
            product.CategoryId,
            product.Price,
            product.Rating,
            product.ReviewsCount
        );
}
