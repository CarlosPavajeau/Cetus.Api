using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.SearchAll;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    bool Enabled,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static Expression<Func<Product, ProductResponse>> Map => product => new ProductResponse(
        product.Id,
        product.Name,
        product.Slug,
        product.Description,
        product.Price,
        product.Stock,
        product.CategoryId,
        product.Enabled,
        product.CreatedAt,
        product.UpdatedAt
    );

    public static ProductResponse FromProduct(Product product) => Map.Compile()(product);
}
