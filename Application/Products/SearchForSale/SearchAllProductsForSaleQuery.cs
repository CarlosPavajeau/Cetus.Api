using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Products.SearchForSale;

public sealed record SearchAllProductsForSaleQuery(
    int Page = 1,
    int PageSize = 20,
    Guid[]? CategoryIds = null,
    string? SearchTerm = null
) : IQuery<PagedResult<SimpleProductForSaleResponse>>;
