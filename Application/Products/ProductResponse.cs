using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid CategoryId,
    string Category,
    bool Enabled,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static Expression<Func<Product, ProductResponse>> Map => product => new ProductResponse(
        product.Id,
        product.Name,
        product.Slug,
        product.Description,
        product.CategoryId,
        product.Category != null ? product.Category.Name : string.Empty,
        product.Enabled,
        product.CreatedAt,
        product.UpdatedAt
    );

    public static ProductResponse FromProduct(Product product) => Map.Compile()(product);
}
