using Application.Abstractions.Messaging;
using Application.Products.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:guid}", async (
            Guid id,
            IQueryHandler<FindProductQuery, ProductResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindProductQuery(id);
            var cacheKey = $"product-{id}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Products);
    }
}
