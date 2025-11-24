using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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
        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == tenant.Id);

        if (request.Statuses is not null && request.Statuses.Length > 0)
        {
            query = query.Where(o => request.Statuses.Contains(o.Status));
        }

        if (request.From is not null)
        {
            query = query.Where(o => o.CreatedAt >= request.From.Value);
        }

        if (request.To is not null)
        {
            // Make upper bound exclusive to include whole 'To' day regardless of time component.
            var toExclusive = request.To.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < toExclusive);
        }

        var total = await query.CountAsync(cancellationToken);

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
