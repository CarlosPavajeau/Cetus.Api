using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Find;

internal sealed class FindCustomerQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<FindCustomerQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(FindCustomerQuery query, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(CustomerResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound(query.Id));
        }

        var stats = await db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == query.Id
                        && o.StoreId == tenant.Id
                        && o.Status != OrderStatus.Canceled
                        && o.Status != OrderStatus.Returned)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalPurchases = g.Count(),
                TotalSpent = g.Sum(o => o.Total),
                MinDate = g.Min(o => (DateTime?)o.CreatedAt),
                MaxDate = g.Max(o => (DateTime?)o.CreatedAt)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stats is null || stats.TotalPurchases == 0)
        {
            return customer;
        }

        double? frequencyDays = stats.TotalPurchases > 1 && stats.MinDate.HasValue && stats.MaxDate.HasValue
            ? (stats.MaxDate.Value - stats.MinDate.Value).TotalDays / (stats.TotalPurchases - 1)
            : null;

        return customer with
        {
            Since = stats.MinDate,
            TotalPurchases = stats.TotalPurchases,
            TotalSpent = stats.TotalSpent,
            PurchaseFrequencyDays = frequencyDays.HasValue ? Math.Round(frequencyDays.Value, 1) : null
        };
    }
}
