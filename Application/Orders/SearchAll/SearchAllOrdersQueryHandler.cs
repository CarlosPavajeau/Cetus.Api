using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.SearchAll;

internal sealed class SearchAllOrdersQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllOrdersQuery, PagedResult<OrderResponse>>
{
    public async Task<Result<PagedResult<OrderResponse>>> Handle(
        SearchAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        int page = request.Page <= 0 ? 1 : request.Page;
        int size = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == tenant.Id);

        if (request.Statuses is not null && request.Statuses.Length > 0)
        {
            var statuses = request.Statuses
                .Select(s => Enum.Parse<OrderStatus>(s, ignoreCase: true))
                .ToArray();

            query = query.Where(o => statuses.Contains(o.Status));
        }

        if (request.From is not null)
        {
            var from = request.From.Value;
            query = query.Where(o => o.CreatedAt >= from);
        }

        if (request.To is not null)
        {
            // Make upper bound exclusive to include whole 'To' day regardless of time component.
            var toExclusive = request.To.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < toExclusive);
        }

        int total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(o => o.Status)
            .ThenBy(o => o.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(OrderResponse.Map)
            .ToListAsync(cancellationToken);

        var payload = PagedResult<OrderResponse>.Create(items, page, size, total);

        return payload;
    }
}
