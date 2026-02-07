using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.FindBySlug;

internal sealed class FindStoreBySlugQueryHandler(IApplicationDbContext db)
    : IQueryHandler<FindStoreBySlugQuery, SimpleStoreResponse>
{
    public async Task<Result<SimpleStoreResponse>> Handle(FindStoreBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .AsNoTracking()
            .Where(s => s.Slug == query.Slug)
            .Select(SimpleStoreResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<SimpleStoreResponse>(StoreErrors.NotFoundBySlug(query.Slug));
        }

        return store;
    }
}
