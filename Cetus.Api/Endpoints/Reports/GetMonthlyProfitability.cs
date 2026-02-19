using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.MonthlyProfitability;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Reports;

internal sealed class GetMonthlyProfitability : IEndpoint
{
    private sealed record Request(
        PeriodPresetParser? Preset = null,
        int? Year = null,
        int? Month = null,
        bool ExcludeCanceled = true,
        bool ExcludeRefunded = true
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/monthly-profitability", async (
            [AsParameters] Request request,
            IQueryHandler<GetMonthlyProfitabilityQuery, MonthlyProfitabilityResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken
        ) =>
        {
            var query = new GetMonthlyProfitabilityQuery(request.Preset, request.Year, request.Month, request.ExcludeCanceled, request.ExcludeRefunded);
            string cacheKey = $"monthly-profitability:t={tenant.Id}:p={query.ResolvedPreset}:y={query.Year}:m={query.Month}" +
                              $":ec={query.ExcludeCanceled}:ef={query.ExcludeRefunded}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5),
                },
                cancellationToken: cancellationToken,
                tags: [$"reports:t={tenant.Id}"]
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Reports);
    }
}
