using System.Globalization;
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
        if (!DateTime.TryParseExact(request.Month, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
        {
            return new OrdersInsightsResponse(0, 0, 0, 0);
        }

        var month = date.Month;
        
        var allOrdersQuery = _context.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAt.Month == month);
        
        var completedOrdersQuery = allOrdersQuery
            .Where(order => order.Status == OrderStatus.Delivered);
        
        var allOrdersCount = await allOrdersQuery.CountAsync(cancellationToken);
        
        var completedOrdersCount = await completedOrdersQuery.CountAsync(cancellationToken);
        
        var currentMonthTotalRevenue = await completedOrdersQuery
            .SumAsync(order => order.Total, cancellationToken);
        
        var itemsInCompletedOrders = await completedOrdersQuery
            .SelectMany(order => order.Items)
            .CountAsync(cancellationToken);
        
        var currentMonthTotalCost = itemsInCompletedOrders * CostPerItem;

        return new OrdersInsightsResponse(
            currentMonthTotalRevenue,
            currentMonthTotalCost,
            allOrdersCount,
            completedOrdersCount);
    }
}
