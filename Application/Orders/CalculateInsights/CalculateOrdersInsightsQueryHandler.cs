using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.CalculateInsights;

internal sealed class CalculateOrdersInsightsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<CalculateOrdersInsightsQuery, OrdersInsightsResponse>
{
    public async Task<Result<OrdersInsightsResponse>> Handle(CalculateOrdersInsightsQuery request,
        CancellationToken cancellationToken)
    {
        if (!DateTime.TryParseExact(request.Month, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
        {
            return OrdersInsightsResponse.Empty;
        }

        var currentMonth = date.Month;
        var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;

        // Current month data
        var currentMonthOrdersQuery = db.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAt.Month == currentMonth && order.StoreId == tenant.Id);

        var currentMonthCompletedOrdersQuery = currentMonthOrdersQuery
            .Where(order => order.Status == OrderStatus.Delivered);

        var currentMonthAllOrdersCount = await currentMonthOrdersQuery.CountAsync(cancellationToken);
        var currentMonthCompletedOrdersCount = await currentMonthCompletedOrdersQuery.CountAsync(cancellationToken);
        var currentMonthTotalRevenue = await currentMonthCompletedOrdersQuery
            .SumAsync(order => order.Total, cancellationToken);

        var customersCount = await currentMonthOrdersQuery
            .Select(order => order.CustomerId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Previous month data
        var previousMonthOrdersQuery = db.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAt.Month == previousMonth && order.StoreId == tenant.Id);

        var previousMonthCompletedOrdersQuery = previousMonthOrdersQuery
            .Where(order => order.Status == OrderStatus.Delivered);

        var previousMonthAllOrdersCount = await previousMonthOrdersQuery.CountAsync(cancellationToken);
        var previousMonthTotalRevenue = await previousMonthCompletedOrdersQuery
            .SumAsync(order => order.Total, cancellationToken);

        // Calculate percentage changes
        var revenuePercentageChange = CalculatePercentageChange(previousMonthTotalRevenue, currentMonthTotalRevenue);
        var ordersCountPercentageChange =
            CalculatePercentageChange(previousMonthAllOrdersCount, currentMonthAllOrdersCount);

        return new OrdersInsightsResponse(
            currentMonthTotalRevenue,
            revenuePercentageChange,
            ordersCountPercentageChange,
            currentMonthAllOrdersCount,
            currentMonthCompletedOrdersCount,
            customersCount
        );
    }

    private static decimal CalculatePercentageChange(decimal previousValue, decimal currentValue)
    {
        if (previousValue == 0)
        {
            return currentValue > 0 ? 100 : 0;
        }

        return Math.Round(((currentValue - previousValue) / previousValue), 2);
    }

    private static decimal CalculatePercentageChange(long previousValue, long currentValue)
    {
        if (previousValue == 0)
        {
            return currentValue > 0 ? 100 : 0;
        }

        return Math.Round(((decimal) (currentValue - previousValue) / previousValue), 2);
    }
}
