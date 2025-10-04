using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.Variants;

public sealed record ProductVariantResponse(
    long Id,
    string Sku,
    decimal Price,
    int Stock,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<VariantOptionValueResponse> OptionValues
)
{
    public static Expression<Func<ProductVariant, ProductVariantResponse>> Map => variant =>
        new ProductVariantResponse(
            variant.Id,
            variant.Sku,
            variant.Price,
            variant.StockQuantity,
            variant.Images.Select(image =>
                new ProductImageResponse(image.Id, image.ImageUrl, image.AltText, image.SortOrder)
            ).ToList(),
            variant.OptionValues.Select(option =>
                new VariantOptionValueResponse(
                    option.OptionValueId,
                    option.ProductOptionValue!.Value,
                    option.ProductOptionValue!.OptionTypeId,
                    option.ProductOptionValue!.ProductOptionType.Name
                )
            ).ToList()
        );
}
