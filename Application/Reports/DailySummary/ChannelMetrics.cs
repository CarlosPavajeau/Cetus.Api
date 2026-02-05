using Domain.Orders;

namespace Application.Reports.DailySummary;

public sealed record ChannelMetrics(
    OrderChannel Channel,
    int OrderCount,
    decimal Revenue,
    decimal Percentage
);
