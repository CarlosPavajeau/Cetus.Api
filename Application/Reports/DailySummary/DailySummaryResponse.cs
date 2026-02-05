namespace Application.Reports.DailySummary;

public sealed record DailySummaryResponse(
    DateTime Date,
    OrdersMetrics Orders,
    RevenueMetrics Revenue,
    TopProductItem? TopProduct,
    IReadOnlyList<ChannelMetrics> ByChannel,
    IReadOnlyList<PaymentStatusMetrics> ByPaymentStatus
);
