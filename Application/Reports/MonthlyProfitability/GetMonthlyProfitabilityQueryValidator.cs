using FluentValidation;

namespace Application.Reports.MonthlyProfitability;

internal sealed class GetMonthlyProfitabilityQueryValidator : AbstractValidator<GetMonthlyProfitabilityQuery>
{
    public GetMonthlyProfitabilityQueryValidator()
    {
        When(q => q.From.HasValue && q.To.HasValue, () =>
        {
            RuleFor(q => q.From)
                .LessThan(q => q.To)
                .WithMessage("La fecha de inicio debe ser anterior a la fecha de fin.");
        });
    }
}
