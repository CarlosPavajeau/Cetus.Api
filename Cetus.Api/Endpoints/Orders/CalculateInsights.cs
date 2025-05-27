using Application.Abstractions.Messaging;
using Application.Orders.CalculateInsights;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class CalculateInsights : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/insights", async (
            [FromQuery] string month,
            IQueryHandler<CalculateOrdersInsightsQuery, OrdersInsightsResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new CalculateOrdersInsightsQuery(month);

            var result = await cache.GetOrCreateAsync(
                $"orders-insights-{month}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5),
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
