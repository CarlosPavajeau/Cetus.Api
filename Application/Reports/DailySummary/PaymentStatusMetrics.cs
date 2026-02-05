using Domain.Orders;

namespace Application.Reports.DailySummary;

public sealed record PaymentStatusMetrics(
    PaymentStatus Status,
    int OrderCount,
    decimal Revenue,
    decimal Percentage
);
