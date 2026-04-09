using FluentValidation;

namespace Application.Reports.ProductProfitabilityRanking;

internal sealed class GetProductProfitabilityRankingQueryValidator : AbstractValidator<GetProductProfitabilityRankingQuery>
{
    private static readonly string[] AllowedSortColumns = ["profit", "margin"];

    private static readonly string[] AllowedDirections = ["asc", "desc"];

    public GetProductProfitabilityRankingQueryValidator()
    {
        RuleFor(q => q)
            .Must(q => q.From is null || q.To is null || q.From.Value.Date <= q.To.Value.Date)
            .WithMessage("La fecha inicial no puede ser mayor que la fecha final.");

        RuleFor(q => q.ResolvedSortBy)
            .Must(sortBy => AllowedSortColumns.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El campo de ordenamiento no es valido.");

        RuleFor(q => q.ResolvedSortDirection)
            .Must(direction => AllowedDirections.Contains(direction, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La direccion de ordenamiento no es valida.");

        RuleFor(q => q.StarMarginThreshold)
            .InclusiveBetween(0m, 1m)
            .WithMessage("El umbral de margen estrella debe estar entre 0 y 1.");

        RuleFor(q => q.ProblematicMarginThreshold)
            .InclusiveBetween(0m, 1m)
            .WithMessage("El umbral de margen problematico debe estar entre 0 y 1.");

        RuleFor(q => q.StarUnitsThreshold)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El umbral de unidades estrella debe ser mayor o igual a 1.");
    }
}
