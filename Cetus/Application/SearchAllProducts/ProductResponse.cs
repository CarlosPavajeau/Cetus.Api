using Cetus.Domain;

namespace Cetus.Application.SearchAllProducts;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool Enabled,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ProductResponse FromProduct(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.Enabled,
            product.CreatedAt,
            product.UpdatedAt
        );
}
