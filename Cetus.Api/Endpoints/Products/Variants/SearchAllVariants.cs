using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Variants;
using Application.Products.Variants.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Variants;

internal sealed class SearchAllVariants : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{productId:guid}/variants", async (
            Guid productId,
            IQueryHandler<SearchAllProductVariantsQuery, IEnumerable<ProductVariantResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductVariantsQuery(productId);

            string cacheKey = CacheKeyBuilder.Build("products", "variants", tenant.Id.ToString(), productId.ToString());

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
