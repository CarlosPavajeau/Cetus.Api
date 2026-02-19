using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.DailySummary;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Reports;

internal sealed class GetDailySummary : IEndpoint
{
    private sealed record Request(DateTime? Date = null);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/daily-summary", async (
            [AsParameters] Request request,
            IQueryHandler<GetDailySummaryQuery, DailySummaryResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken
        ) =>
        {
            var effectiveDate = request.Date ?? DateTime.UtcNow;
            var effectiveQuery = new GetDailySummaryQuery(Date: effectiveDate);

            var result = await cache.GetOrCreateAsync(
                $"daily-summary-{effectiveDate:yyyyMMdd}-{tenant.Id}",
                async token => await handler.Handle(effectiveQuery, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5),
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        });
    }
}
