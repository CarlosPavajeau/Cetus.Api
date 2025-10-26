using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.Find;

internal sealed class FindStoreQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindStoreQuery, SimpleStoreResponse>
{
    public async Task<Result<SimpleStoreResponse>> Handle(FindStoreQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.CustomDomain) && string.IsNullOrEmpty(query.Slug))
        {
            return Result.Failure<SimpleStoreResponse>(StoreErrors.InvalidQuery());
        }

        var queryable = context.Stores.AsNoTracking();
        SimpleStoreResponse? store = null;

        if (!string.IsNullOrEmpty(query.CustomDomain))
        {
            store = await queryable.Where(s => s.CustomDomain == query.CustomDomain)
                .Select(SimpleStoreResponse.Map)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (store is not null)
        {
            return store;
        }

        if (!string.IsNullOrEmpty(query.Slug))
        {
            store = await queryable.Where(s => s.Slug == query.Slug)
                .Select(SimpleStoreResponse.Map)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (store is null)
        {
            return Result.Failure<SimpleStoreResponse>(StoreErrors.NotFound(query.CustomDomain, query.Slug));
        }

        return store;
    }
}
