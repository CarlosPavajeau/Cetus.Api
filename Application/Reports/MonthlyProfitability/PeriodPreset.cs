using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Application.Reports.MonthlyProfitability;

[JsonConverter(typeof(JsonStringEnumConverter<PeriodPreset>))]
public enum PeriodPreset
{
    ThisMonth,
    LastMonth,
    SpecificMonth
}

public sealed class PeriodPresetParser : IParsable<PeriodPresetParser>
{
    public PeriodPreset Value { get; private init; }

    public static PeriodPresetParser Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"'{s}' is not a valid period preset.");
        }

        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider,
        [MaybeNullWhen(false)] out PeriodPresetParser result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        // Try snake_case mapping first
        PeriodPreset? preset = s switch
        {
            "this_month" => PeriodPreset.ThisMonth,
            "last_month" => PeriodPreset.LastMonth,
            "specific_month" => PeriodPreset.SpecificMonth,
            _ => null
        };

        // Fall back to standard enum parsing (PascalCase)
        if (preset is null && Enum.TryParse<PeriodPreset>(s, true, out var parsed))
        {
            preset = parsed;
        }

        if (preset is null)
        {
            return false;
        }

        result = new PeriodPresetParser { Value = preset.Value };
        return true;
    }
}
