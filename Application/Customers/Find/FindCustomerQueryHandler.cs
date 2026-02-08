using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

        var since = await db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == query.Id && o.StoreId == tenant.Id)
            .MinAsync(o => (DateTime?)o.CreatedAt, cancellationToken);

        return customer with { Since = since };
    }
}
