using System.Linq.Expressions;
using Application.Products.Options;
using Application.Products.Variants;
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
    string CategorySlug,
    bool Enabled,
    Guid StoreId,
    IEnumerable<ProductVariantResponse> Variants,
    IEnumerable<ProductOptionTypeResponse> AvailableOptions)
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
            product.Category.Slug,
            product.Enabled,
            product.StoreId,
            // Variants - will be populated by the updated handler query
            Enumerable.Empty<ProductVariantResponse>(),
            // Available options - will be populated by the updated handler query  
            Enumerable.Empty<ProductOptionTypeResponse>()
        );
}
