using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.SearchAll;

internal sealed class SearchAllOrdersQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllOrdersQuery, IEnumerable<OrderResponse>>
{
    public async Task<Result<IEnumerable<OrderResponse>>> Handle(SearchAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.Status)
            .ThenBy(order => order.CreatedAt)
            .Where(o => o.StoreId == tenant.Id)
            .Select(OrderResponse.Map)
            .ToListAsync(cancellationToken);

        return orders;
    }
}
