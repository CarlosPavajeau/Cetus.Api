namespace Cetus.Orders.Application.CalculateInsights;

public sealed record OrdersInsightsResponse(
    decimal CurrentMonthTotal,
    decimal CurrentMonthCost,
    long AllOrdersCount,
    long CompletedOrdersCount);
