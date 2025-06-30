using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.TopSelling;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class TopSelling : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/top-selling", async (
            IQueryHandler<GetTopSellingProductsQuery, IEnumerable<TopSellingProductResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTopSellingProductsQuery();

            var result = await cache.GetOrCreateAsync(
                $"products-top-selling-${tenant.Id}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(20),
                    LocalCacheExpiration = TimeSpan.FromMinutes(20)
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Products);
    }
}
