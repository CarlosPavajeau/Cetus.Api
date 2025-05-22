using Application.Abstractions.Messaging;
using Application.Products.SearchForSale;
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
            IQueryHandler<SearchProductSuggestionsQuery, IEnumerable<ProductResponse>> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchProductSuggestionsQuery(productId);
            var cacheKey = $"suggestions-{productId}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous();
    }
}
