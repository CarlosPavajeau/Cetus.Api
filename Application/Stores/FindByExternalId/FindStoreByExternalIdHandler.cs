using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.FindByExternalId;

internal sealed class FindStoreByExternalIdHandler(IApplicationDbContext db)
    : IQueryHandler<FindStoreByExternalId, StoreResponse>
{
    public async Task<Result<StoreResponse>> Handle(FindStoreByExternalId query, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .AsNoTracking()
            .Where(s => s.ExternalId == query.ExternalId)
            .Select(StoreResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<StoreResponse>(StoreErrors.NotFoundById(query.ExternalId));
        }

        return store;
    }
}
