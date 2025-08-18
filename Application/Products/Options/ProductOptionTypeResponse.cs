using System.Linq.Expressions;
using Domain.Products;

namespace Application.Products.Options;

public sealed record ProductOptionTypeResponse(long Id, string Name, string[] Values)
{
    public static Expression<Func<ProductOptionType, ProductOptionTypeResponse>> Map => optionType =>
        new ProductOptionTypeResponse(
            optionType.Id,
            optionType.Name,
            optionType.ProductOptionValues.Select(v => v.Value).ToArray()
        );
}
