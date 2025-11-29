using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.SearchSuggestions;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class SearchSuggestions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/suggestions", async (
            [FromQuery] Guid productId,
            IQueryHandler<SearchProductSuggestionsQuery, IEnumerable<SimpleProductForSaleResponse>> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchProductSuggestionsQuery(productId);
            string cacheKey = $"suggestions-{productId}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
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
