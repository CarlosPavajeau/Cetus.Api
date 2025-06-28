namespace Application.Orders.CalculateInsights;

public sealed record OrdersInsightsResponse(
    decimal CurrentMonthTotal,
    decimal CurrentMonthCost,
    long AllOrdersCount,
    long CompletedOrdersCount,
    decimal RevenuePercentageChange,
    decimal OrdersCountPercentageChange)
{
    public static OrdersInsightsResponse Empty => new(0, 0, 0, 0, 0, 0);
}
