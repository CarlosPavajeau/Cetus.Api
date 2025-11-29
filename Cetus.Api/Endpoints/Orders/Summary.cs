using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Summary;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Summary : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/summary", async (
            [FromQuery] string month,
            IQueryHandler<GetOrdersSummaryQuery, IEnumerable<OrderSummaryResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrdersSummaryQuery(month);
            string cacheKey = $"orders-summary-{month}-{tenant.Id}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
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
