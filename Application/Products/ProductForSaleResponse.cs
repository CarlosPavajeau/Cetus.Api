using System.Linq.Expressions;
using Application.Products.Options;
using Application.Products.Variants;
using Domain.Products;

namespace Application.Products;

public sealed record ProductForSaleResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Rating,
    int ReviewsCount,
    Guid CategoryId,
    string? Category,
    string CategorySlug,
    bool Enabled,
    Guid StoreId,
    IEnumerable<ProductVariantResponse> Variants,
    IEnumerable<ProductOptionTypeResponse> AvailableOptions)
{
    public static Expression<Func<Product, ProductForSaleResponse>> Map => product =>
        new ProductForSaleResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Rating,
            product.ReviewsCount,
            product.CategoryId,
            product.Category!.Name,
            product.Category.Slug,
            product.Enabled,
            product.StoreId,
            // Variants - will be populated by the updated handler query
            Enumerable.Empty<ProductVariantResponse>(),
            // Available options - will be populated by the updated handler query  
            Enumerable.Empty<ProductOptionTypeResponse>()
        );

    public static ProductForSaleResponse FromProduct(Product product) => Map.Compile()(product);
}
