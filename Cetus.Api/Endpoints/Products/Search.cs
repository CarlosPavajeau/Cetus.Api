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
            var query = new SearchProductsQuery(searchTerm);

            var result = await cache.GetOrCreateAsync(
                $"{tenant.Id}-products-search-{searchTerm}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(1),
                    LocalCacheExpiration = TimeSpan.FromHours(1)
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products).AllowAnonymous();
    }
}
