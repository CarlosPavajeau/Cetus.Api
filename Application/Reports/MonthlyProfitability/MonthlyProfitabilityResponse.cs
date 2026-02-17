namespace Application.Reports.MonthlyProfitability;

public sealed record MonthlyProfitabilityResponse(
    ProfitabilitySummary Summary,
    MonthComparison? PreviousMonthComparison,
    IReadOnlyList<MonthlyTrend> Trend,
    IReadOnlyList<ProductCostWarning> ProductsWithoutCost
);
