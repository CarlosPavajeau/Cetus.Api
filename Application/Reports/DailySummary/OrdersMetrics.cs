namespace Application.Reports.DailySummary;

public sealed record OrdersMetrics(
    int Total,
    int Confirmed,
    int Pending,
    int AwaitingVerification,
    int Rejected,
    int Canceled
);
