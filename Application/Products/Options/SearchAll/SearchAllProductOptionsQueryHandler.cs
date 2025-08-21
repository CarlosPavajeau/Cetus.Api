using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Options.SearchAll;

internal sealed class SearchAllProductOptionsQueryHandler(IApplicationDbContext db) : IQueryHandler<SearchAllProductOptionsQuery, IEnumerable<ProductOptionResponse>>
{
    public async Task<Result<IEnumerable<ProductOptionResponse>>> Handle(SearchAllProductOptionsQuery query,
        CancellationToken cancellationToken)
    {
        var productOptions = await db.ProductOptions
            .AsNoTracking()
            .Where(p => p.ProductId == query.ProductId)
            .Select(ProductOptionResponse.Map)
            .ToListAsync(cancellationToken);

        return productOptions;
    }
}
