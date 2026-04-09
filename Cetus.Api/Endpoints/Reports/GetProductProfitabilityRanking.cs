using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.ProductProfitabilityRanking;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Reports;

internal sealed class GetProductProfitabilityRanking : IEndpoint
{
    private sealed record Request(
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
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/product-profitability-ranking", async (
            [AsParameters] Request request,
            IQueryHandler<GetProductProfitabilityRankingQuery, IReadOnlyList<ProductProfitabilityItem>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken
        ) =>
        {
            var query = new GetProductProfitabilityRankingQuery(
                From: request.From,
                To: request.To,
                CategoryId: request.CategoryId,
                SortBy: request.SortBy,
                SortDirection: request.SortDirection,
                ExcludeCanceled: request.ExcludeCanceled,
                ExcludeRefunded: request.ExcludeRefunded,
                StarMarginThreshold: request.StarMarginThreshold,
                StarUnitsThreshold: request.StarUnitsThreshold,
                ProblematicMarginThreshold: request.ProblematicMarginThreshold
            );

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new("categoryId", query.CategoryId?.ToString() ?? string.Empty),
                new("excludeCanceled", query.ExcludeCanceled.ToString()),
                new("excludeRefunded", query.ExcludeRefunded.ToString()),
                new("from", query.From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty),
                new("problematicMarginThreshold", query.ProblematicMarginThreshold.ToString(CultureInfo.InvariantCulture)),
                new("sortBy", query.ResolvedSortBy),
                new("sortDirection", query.ResolvedSortDirection),
                new("starMarginThreshold", query.StarMarginThreshold.ToString(CultureInfo.InvariantCulture)),
                new("starUnitsThreshold", query.StarUnitsThreshold.ToString(CultureInfo.InvariantCulture)),
                new("to", query.To?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty)
            };

            string cacheKey = CacheKeyBuilder.BuildWithQuery(
                "reports",
                queryParams,
                "product-profitability-ranking",
                tenant.Id.ToString()
            );

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                },
                cancellationToken: cancellationToken,
                tags: [$"reports:t={tenant.Id}"]
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Reports);
    }
}
