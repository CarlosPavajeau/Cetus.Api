namespace Application.Reports.DailySummary;

public sealed record RevenueMetrics(
    decimal Confirmed,
    decimal Pending,
    decimal Total
);
