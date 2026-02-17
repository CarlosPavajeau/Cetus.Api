namespace Application.Reports.MonthlyProfitability;

public sealed record ProfitabilitySummary(
    decimal TotalSales,
    decimal TotalCost,
    decimal GrossProfit,
    decimal MarginPercentage
);
