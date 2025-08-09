using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.Find;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    IEnumerable<ProductImageResponse> Images,
    decimal Rating,
    int ReviewsCount,
    Guid CategoryId,
    string? Category,
    Guid StoreId)
{
    public static ProductResponse FromProduct(Product product) => Map.Compile()(product);

    public static Expression<Func<Product, ProductResponse>> Map => product =>
        new ProductResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Price,
            product.Stock,
            product.ImageUrl,
            product.Images.Select(img => new ProductImageResponse(img.Id, img.ImageUrl, img.AltText, img.SortOrder)),
            product.Rating,
            product.ReviewsCount,
            product.CategoryId,
            product.Category!.Name,
            product.StoreId
        );
}
