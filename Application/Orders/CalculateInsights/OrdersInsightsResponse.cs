namespace Application.Orders.CalculateInsights;

public sealed record OrdersInsightsResponse(
    decimal CurrentMonthTotal,
    decimal RevenuePercentageChange,
    decimal OrdersCountPercentageChange,
    long AllOrdersCount,
    long CompletedOrdersCount,
    long CustomersCount,
    decimal CustomerPercentageChange)
{
    public static OrdersInsightsResponse Empty => new(0, 0, 0, 0, 0, 0, 0);
}
