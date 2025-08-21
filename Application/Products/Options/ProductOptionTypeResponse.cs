using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.Options;

public sealed record ProductOptionTypeValueResponse(long Id, string Value);

public sealed record ProductOptionTypeResponse(long Id, string Name, IEnumerable<ProductOptionTypeValueResponse> Values)
{
    public static Expression<Func<ProductOptionType, ProductOptionTypeResponse>> Map => optionType =>
        new ProductOptionTypeResponse(
            optionType.Id,
            optionType.Name,
            optionType.ProductOptionValues
                .OrderBy(v => v.Value)
                .Select(v => new ProductOptionTypeValueResponse(v.Id, v.Value))
                .ToList()
        );
}
