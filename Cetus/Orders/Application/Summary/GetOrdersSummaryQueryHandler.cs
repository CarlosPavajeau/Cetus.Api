using System.Globalization;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Orders.Application.Summary;

internal sealed class
    GetOrdersSummaryQueryHandler : IRequestHandler<GetOrdersSummaryQuery, IEnumerable<OrderSummaryResponse>>
{
    private readonly CetusDbContext _context;

    public GetOrdersSummaryQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderSummaryResponse>> Handle(GetOrdersSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (!DateTime.TryParseExact(request.Month, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
        {
            return [];
        }

        var orders = await _context.Orders
            .Where(o => o.CreatedAt.Month == date.Month && o.CreatedAt.Year == date.Year)
            .Select(order => new OrderSummaryResponse(order.Id, order.Status, order.CreatedAt))
            .ToListAsync(cancellationToken);

        return orders;
    }
}
