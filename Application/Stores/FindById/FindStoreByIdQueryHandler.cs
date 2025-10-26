using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.FindById;

internal sealed class FindStoreByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindStoreByIdQuery, SimpleStoreResponse>
{
    public async Task<Result<SimpleStoreResponse>> Handle(FindStoreByIdQuery query, CancellationToken cancellationToken)
    {
        var store = await context.Stores
            .AsNoTracking()
            .Where(s => s.Id == query.Id)
            .Select(SimpleStoreResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<SimpleStoreResponse>(StoreErrors.NotFoundById(query.Id));
        }

        return store;
    }
}
