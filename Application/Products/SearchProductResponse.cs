using System.Linq.Expressions;
using Application.Products.Variants;
using Domain.Products;

namespace Application.Products;

public sealed record SearchProductResponse(
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
    IEnumerable<SearchProductVariantResponse> Variants
)
{
    public static Expression<Func<Product, SearchProductResponse>> Map => product =>
        new SearchProductResponse(
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
            product.Variants.Select(v =>
                new SearchProductVariantResponse(
                    v.Id,
                    v.Sku,
                    v.Price,
                    v.Stock,
                    v.Images
                        .OrderBy(i => i.SortOrder)
                        .ThenBy(i => i.Id)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    v.OptionValues
                        .Select(ov => new VariantOptionValueResponse(
                            ov.OptionValueId,
                            ov.ProductOptionValue!.Value,
                            ov.ProductOptionValue.OptionTypeId,
                            ov.ProductOptionValue.ProductOptionType!.Name
                        ))
                        .ToList()
                ))
        );
}
