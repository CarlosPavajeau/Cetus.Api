namespace Application.Reports.MonthlyProfitability;

public sealed record MonthlyProfitabilityResponse(
    ProfitabilitySummary Summary,
    ProfitabilitySummary? ComparisonSummary,
    MonthComparison? PreviousMonthComparison,
    IReadOnlyList<MonthlyTrend> Trend,
    IReadOnlyList<ProductCostWarning> ProductsWithoutCost
);
