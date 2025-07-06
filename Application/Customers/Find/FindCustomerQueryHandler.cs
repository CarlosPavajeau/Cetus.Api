using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Find;

internal sealed class FindCustomerQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindCustomerQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(FindCustomerQuery query, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(CustomerResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound(query.Id));
        }

        return customer;
    }
}
