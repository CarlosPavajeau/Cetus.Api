using Application.Abstractions.Messaging;

namespace Application.Reports.DailySummary;

public sealed record GetDailySummaryQuery(
    DateTime? Date = null
) : IQuery<DailySummaryResponse>;
