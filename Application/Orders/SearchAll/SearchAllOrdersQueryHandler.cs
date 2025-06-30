using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.SearchAll;

internal sealed class SearchAllOrdersQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<SearchAllOrdersQuery, IEnumerable<OrderResponse>>
{
    public async Task<Result<IEnumerable<OrderResponse>>> Handle(SearchAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.City)
            .ThenInclude(c => c!.State)
            .OrderByDescending(order => order.Status)
            .ThenBy(order => order.CreatedAt)
            .Where(o => o.StoreId == tenant.Id)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderResponse.FromOrder).ToList();
    }
}
