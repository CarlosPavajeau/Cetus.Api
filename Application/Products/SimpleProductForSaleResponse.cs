using System.Linq.Expressions;
using Application.Products.Variants;
using Domain.Products;

namespace Application.Products;

public sealed record SimpleProductForSaleResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ImageUrl,
    Guid CategoryId,
    decimal Price,
    int Stock,
    decimal Rating,
    int ReviewsCount,
    long VariantId,
    IEnumerable<VariantOptionValueResponse> OptionValues)
{
    public static Expression<Func<ProductVariant, SimpleProductForSaleResponse>> Map => variant =>
        new SimpleProductForSaleResponse(
            variant.ProductId,
            variant.Product!.Name,
            variant.Product!.Slug,
            variant.Images.OrderBy(i => i.SortOrder).FirstOrDefault()!.ImageUrl,
            variant.Product!.CategoryId,
            variant.Price,
            variant.Stock,
            variant.Product!.Rating,
            variant.Product!.ReviewsCount,
            variant.Id,
            variant.OptionValues
                .Select(ov => new VariantOptionValueResponse(
                    ov.OptionValueId,
                    ov.ProductOptionValue!.Value,
                    ov.ProductOptionValue.OptionTypeId,
                    ov.ProductOptionValue.ProductOptionType!.Name
                ))
                .ToList()
        );
}
