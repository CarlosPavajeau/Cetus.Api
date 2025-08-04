using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.SearchAllPopularProducts;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class SearchAllPopular : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/popular", async (
            IQueryHandler<SearchAllPopularProductsQuery, IEnumerable<SimpleProductForSaleResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllPopularProductsQuery();

            var result = await cache.GetOrCreateAsync(
                $"products-popular-${tenant.Id}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(5),
                    LocalCacheExpiration = TimeSpan.FromHours(5)
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Products);
    }
}
