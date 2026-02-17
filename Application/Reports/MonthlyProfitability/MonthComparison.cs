namespace Application.Reports.MonthlyProfitability;

public sealed record MonthComparison(
    decimal SalesChange,
    decimal ProfitChange,
    decimal MarginChange
);
