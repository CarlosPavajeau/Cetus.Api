using FluentValidation;

namespace Application.Reports.MonthlyProfitability;

internal sealed class GetMonthlyProfitabilityQueryValidator : AbstractValidator<GetMonthlyProfitabilityQuery>
{
    public GetMonthlyProfitabilityQueryValidator()
    {
        RuleFor(q => q.Preset)
            .IsInEnum()
            .WithMessage("El tipo de periodo no es valido.");

        When(q => q.Preset == PeriodPreset.SpecificMonth, () =>
        {
            RuleFor(q => q.Year)
                .NotNull()
                .WithMessage("El anio es requerido cuando se selecciona un mes especifico.")
                .InclusiveBetween(2020, DateTime.UtcNow.Year)
                .WithMessage("El anio debe estar entre 2020 y el anio actual.");

            RuleFor(q => q.Month)
                .NotNull()
                .WithMessage("El mes es requerido cuando se selecciona un mes especifico.")
                .InclusiveBetween(1, 12)
                .WithMessage("El mes debe estar entre 1 y 12.");
        });
    }
}
