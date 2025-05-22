using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.CalculateInsights;

internal sealed class CalculateOrdersInsightsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<CalculateOrdersInsightsQuery, OrdersInsightsResponse>
{
    private const decimal CostPerItem = 2000m;

    public async Task<Result<OrdersInsightsResponse>> Handle(CalculateOrdersInsightsQuery request,
        CancellationToken cancellationToken)
    {
        if (!DateTime.TryParseExact(request.Month, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
        {
            return OrdersInsightsResponse.Empty;
        }

        var month = date.Month;
        
        var allOrdersQuery = context.Orders
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
