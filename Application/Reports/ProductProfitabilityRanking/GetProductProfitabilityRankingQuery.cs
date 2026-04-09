using Application.Abstractions.Messaging;

namespace Application.Reports.ProductProfitabilityRanking;

public sealed record GetProductProfitabilityRankingQuery(
    DateTime? From = null,
    DateTime? To = null,
    Guid? CategoryId = null,
    string? SortBy = null,
    string? SortDirection = null,
    bool ExcludeCanceled = true,
    bool ExcludeRefunded = true,
    decimal StarMarginThreshold = 0.30m,
    int StarUnitsThreshold = 10,
    decimal ProblematicMarginThreshold = 0.10m
) : IQuery<IReadOnlyList<ProductProfitabilityItem>>
{
    public string ResolvedSortBy => string.IsNullOrWhiteSpace(SortBy) ? "profit" : SortBy.Trim();

    public string ResolvedSortDirection =>
        string.IsNullOrWhiteSpace(SortDirection) ? "desc" : SortDirection.Trim();
}
