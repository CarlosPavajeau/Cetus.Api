using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Summary;

internal sealed class
    GetOrdersSummaryQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<GetOrdersSummaryQuery, IEnumerable<OrderSummaryResponse>>
{
    public async Task<Result<IEnumerable<OrderSummaryResponse>>> Handle(GetOrdersSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (!DateTime.TryParseExact(request.Month, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
        {
            return Result.Success<IEnumerable<OrderSummaryResponse>>([]);
        }

        var orders = await context.Orders
            .Where(o => o.CreatedAt.Month == date.Month && o.CreatedAt.Year == date.Year && o.StoreId == tenant.Id)
            .Select(order => new OrderSummaryResponse(order.Id, order.Status, order.CreatedAt))
            .ToListAsync(cancellationToken);

        return orders;
    }
}
