using Application.Abstractions.Messaging;

namespace Application.Reports.MonthlyProfitability;

public sealed record GetMonthlyProfitabilityQuery(
    PeriodPreset Preset = PeriodPreset.ThisMonth,
    int? Year = null,
    int? Month = null,
    bool ExcludeCanceled = true,
    bool ExcludeRefunded = true
) : IQuery<MonthlyProfitabilityResponse>;
