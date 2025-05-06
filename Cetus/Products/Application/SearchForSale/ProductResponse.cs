using Cetus.Products.Domain;

namespace Cetus.Products.Application.SearchForSale;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    Guid CategoryId,
    string? Category)
{
    public static ProductResponse FromProduct(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.ImageUrl,
            product.CategoryId,
            product.Category?.Name
        );
}
