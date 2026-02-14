using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.FindByPhone;

internal sealed class FindCustomerByPhoneQueryHandler(IApplicationDbContext db)
    : IQueryHandler<FindCustomerByPhoneQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(FindCustomerByPhoneQuery query,
        CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .Where(c => c.Phone == query.Phone)
            .Select(CustomerResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFoundByPhone(query.Phone));
        }

        return customer;
    }
}
