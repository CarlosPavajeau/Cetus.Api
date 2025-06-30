using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.Find;

internal sealed class FindStoreQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindStoreQuery, StoreResponse>
{
    public async Task<Result<StoreResponse>> Handle(FindStoreQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.CustomDomain) && string.IsNullOrEmpty(query.Slug))
        {
            return Result.Failure<StoreResponse>(StoreErrors.InvalidQuery());
        }

        var queryable = context.Stores.AsNoTracking();

        if (!string.IsNullOrEmpty(query.CustomDomain))
        {
            queryable = queryable.Where(s => s.CustomDomain == query.CustomDomain);
        }

        if (!string.IsNullOrEmpty(query.Slug))
        {
            queryable = queryable.Where(s => s.Slug == query.Slug);
        }

        var store = await queryable.FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<StoreResponse>(StoreErrors.NotFound(query.CustomDomain, query.Slug));
        }

        return StoreResponse.FromStore(store);
    }
}
