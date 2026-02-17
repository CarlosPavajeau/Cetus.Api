using Application.Abstractions.Messaging;

namespace Application.Reports.MonthlyProfitability;

public sealed record GetMonthlyProfitabilityQuery(
    DateTime? From = null,
    DateTime? To = null,
    bool ExcludeCanceled = true,
    bool ExcludeRefunded = true
) : IQuery<MonthlyProfitabilityResponse>;
