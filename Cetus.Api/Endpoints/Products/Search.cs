using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.Search;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class Search : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/search", async (
            string searchTerm,
            IQueryHandler<SearchProductsQuery, IEnumerable<SearchProductResponse>> handler,
            ITenantContext tenant,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
#pragma warning disable CA1308
            string normalizedSearchTerm = searchTerm.Trim().ToLowerInvariant();
            var query = new SearchProductsQuery(normalizedSearchTerm);

            var result = await cache.GetOrCreateAsync(
                $"{tenant.Id}-products-search-{normalizedSearchTerm}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                },
                tags: ["products", $"tenant-{tenant.Id}"],
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products).AllowAnonymous();
    }
}
