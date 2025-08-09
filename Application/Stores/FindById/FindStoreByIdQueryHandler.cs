using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.FindById;

internal sealed class FindStoreByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<FindStoreByIdQuery, StoreResponse>
{
    public async Task<Result<StoreResponse>> Handle(FindStoreByIdQuery query, CancellationToken cancellationToken)
    {
        var store = await context.Stores
            .AsNoTracking()
            .Where(s => s.Id == query.Id)
            .Select(StoreResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<StoreResponse>(StoreErrors.NotFoundById(query.Id.ToString()));
        }

        return store;
    }
}
