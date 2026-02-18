using Application.Abstractions.Messaging;

namespace Application.Reports.MonthlyProfitability;

public sealed record GetMonthlyProfitabilityQuery(
    PeriodPresetParser? Preset = null,
    int? Year = null,
    int? Month = null,
    bool ExcludeCanceled = true,
    bool ExcludeRefunded = true
) : IQuery<MonthlyProfitabilityResponse>
{
    public PeriodPreset ResolvedPreset => Preset?.Value ?? PeriodPreset.ThisMonth;
}
