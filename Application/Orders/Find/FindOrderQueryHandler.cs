using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Find;

internal sealed class FindOrderQueryHandler(IApplicationDbContext db)
    : IQueryHandler<FindOrderQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(FindOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.Id)
            .Select(OrderResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound(query.Id));
        }

        return order;
    }
}
