using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Search;

internal sealed class SearchProductsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchProductsQuery, IEnumerable<SearchProductResponse>>
{
    private const int MaxResults = 30;

    public async Task<Result<IEnumerable<SearchProductResponse>>> Handle(SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Where(p =>
                p.DeletedAt == null && p.StoreId == tenant.Id &&
                p.SearchVector!.Matches(EF.Functions.PhraseToTsQuery("spanish", query.SearchTerm))
            )
            .Take(MaxResults)
            .Select(SearchProductResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
