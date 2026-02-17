using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.MonthlyProfitability;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Reports;

internal sealed class GetMonthlyProfitability : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/monthly-profitability", async (
            [AsParameters] GetMonthlyProfitabilityQuery query,
            IQueryHandler<GetMonthlyProfitabilityQuery, MonthlyProfitabilityResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken
        ) =>
        {
            string cacheKey = $"monthly-profitability-{tenant.Id}-{query.From:yyyyMMdd}-{query.To:yyyyMMdd}" +
                              $"-{query.ExcludeCanceled}-{query.ExcludeRefunded}";

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
