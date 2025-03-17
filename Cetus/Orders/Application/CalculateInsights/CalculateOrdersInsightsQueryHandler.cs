using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Orders.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Orders.Application.CalculateInsights;

internal sealed class
    CalculateOrdersInsightsQueryHandler : IRequestHandler<CalculateOrdersInsightsQuery, OrdersInsightsResponse>
{
    private const decimal CostPerItem = 2000m;

    private readonly CetusDbContext _context;

    public CalculateOrdersInsightsQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<OrdersInsightsResponse> Handle(CalculateOrdersInsightsQuery request,
        CancellationToken cancellationToken)
    {
        var currentMonthTotal = await _context.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAt.Month == DateTime.Now.Month && order.Status == OrderStatus.Delivered)
            .SumAsync(order => order.Total, cancellationToken);

        var itemsPerMonth = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(order => order.CreatedAt.Month == DateTime.Now.Month && order.Status == OrderStatus.Delivered)
            .SelectMany(order => order.Items)
            .CountAsync(cancellationToken);

        var currentMonthCost = itemsPerMonth * CostPerItem;

        return new OrdersInsightsResponse(currentMonthTotal, currentMonthCost);
    }
}
