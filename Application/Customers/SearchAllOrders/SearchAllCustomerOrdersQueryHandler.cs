using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.SearchAllOrders;

internal sealed class SearchAllCustomerOrdersQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllCustomerOrdersQuery, PagedResult<SimpleOrderResponse>>
{
    public async Task<Result<PagedResult<SimpleOrderResponse>>> Handle(SearchAllCustomerOrdersQuery query,
        CancellationToken cancellationToken)
    {
        int page = query.Page <= 0 ? 1 : query.Page;
        int size = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == tenant.Id && o.CustomerId == query.CustomerId);

        int total = await ordersQuery.CountAsync(cancellationToken);
        var items = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(o => new SimpleOrderResponse(
                o.Id,
                o.OrderNumber,
                o.Status,
                o.Address,
                o.Subtotal,
                o.Discount,
                o.Total,
                o.CustomerId,
                o.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var payload = PagedResult<SimpleOrderResponse>.Create(items, page, size, total);

        return payload;
    }
}
