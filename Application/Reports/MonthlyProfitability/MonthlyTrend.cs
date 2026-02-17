namespace Application.Reports.MonthlyProfitability;

public sealed record MonthlyTrend(
    int Year,
    int Month,
    decimal TotalSales,
    decimal TotalCost,
    decimal GrossProfit,
    decimal MarginPercentage
);
