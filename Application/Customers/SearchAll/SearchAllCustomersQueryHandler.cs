using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.SearchAll;

internal sealed class SearchAllCustomersQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllCustomersQuery, PagedResult<CustomerSummaryResponse>>
{
    public async Task<Result<PagedResult<CustomerSummaryResponse>>> Handle(
        SearchAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        int page = request.Page <= 0 ? 1 : request.Page;
        int size = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == tenant.Id)
            .GroupBy(o => new { o.Customer!.Id, o.Customer.Name, o.Customer.Phone, o.Customer.Email })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                g.Key.Phone,
                g.Key.Email,
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(o => o.Total),
                LastPurchase = g.Max(o => (DateTime?)o.CreatedAt)
            });

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = $"%{request.Search}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, search) ||
                EF.Functions.ILike(x.Phone, search));
        }

        int total = await query.CountAsync(cancellationToken);

        var sorted = request.SortBy switch
        {
            CustomerSortBy.Name => query.OrderBy(x => x.Name),
            CustomerSortBy.LastPurchase => query.OrderByDescending(x => x.LastPurchase),
            _ => query.OrderByDescending(x => x.TotalSpent)
        };

        var items = await sorted
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new CustomerSummaryResponse(
                x.Id,
                x.Name,
                x.Phone,
                x.Email,
                x.TotalOrders,
                x.TotalSpent,
                x.LastPurchase))
            .ToListAsync(cancellationToken);

        return PagedResult<CustomerSummaryResponse>.Create(items, page, size, total);
    }
}
