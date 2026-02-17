using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.MonthlyProfitability;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

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
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken
        ) =>
        {
            var now = dateTimeProvider.UtcNow;
            var from = query.From?.Date ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = query.To?.Date ?? now.Date.AddDays(1);

            string cacheKey = $"monthly-profitability-{tenant.Id}-{from:yyyyMMdd}-{to:yyyyMMdd}" +
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
