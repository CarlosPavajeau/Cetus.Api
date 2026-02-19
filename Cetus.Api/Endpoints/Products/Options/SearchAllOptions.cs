using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Options;
using Application.Products.Options.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Options;

internal sealed class SearchAllOptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{productId:guid}/options", async (
            Guid productId,
            IQueryHandler<SearchAllProductOptionsQuery, IEnumerable<ProductOptionResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductOptionsQuery(productId);

            string cacheKey = CacheKeyBuilder.Build(
                "products",
                "options",
                tenant.Id.ToString(),
                productId.ToString()
            );

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
