using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.Options;

public sealed record ProductOptionResponse(Guid ProductId, long OptionTypeId, ProductOptionTypeResponse OptionType)
{
    public static Expression<Func<ProductOption, ProductOptionResponse>> Map => option =>
        new ProductOptionResponse(
            option.ProductId,
            option.OptionTypeId,
            new ProductOptionTypeResponse(
                option.ProductOptionType!.Id,
                option.ProductOptionType.Name,
                option.ProductOptionType.ProductOptionValues
                    .OrderBy(v => v.Value)
                    .Select(v => new ProductOptionTypeValueResponse(v.Id, v.Value))
                    .ToList()
            )
        );
}
