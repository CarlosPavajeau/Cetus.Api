using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.FindByDomain;

internal sealed class FindStoreByDomainQueryHandler(IApplicationDbContext db)
    : IQueryHandler<FindStoreByDomainQuery, SimpleStoreResponse>
{
    public async Task<Result<SimpleStoreResponse>> Handle(FindStoreByDomainQuery query,
        CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .AsNoTracking()
            .Where(s => s.CustomDomain != null && s.CustomDomain == query.Domain)
            .Select(SimpleStoreResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<SimpleStoreResponse>(StoreErrors.NotFoundByCustomDomain(query.Domain));
        }

        return store;
    }
}
