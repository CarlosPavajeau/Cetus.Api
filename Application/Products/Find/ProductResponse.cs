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
    decimal Rating,
    int ReviewsCount,
    Guid CategoryId,
    string? Category)
{
    public static ProductResponse FromProduct(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Price,
            product.Stock,
            product.ImageUrl,
            product.Rating,
            product.ReviewsCount,
            product.CategoryId,
            product.Category?.Name
        );
}
