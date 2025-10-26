using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stores/by-domain/{domain}", async (
            string domain,
            IQueryHandler<FindStoreQuery, SimpleStoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreQuery(domain, null);
            var cacheKey = $"store-${domain}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Stores);

        app.MapGet("stores/by-slug/{slug}", async (
            string slug,
            IQueryHandler<FindStoreQuery, SimpleStoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreQuery(null, slug);
            var cacheKey = $"store-${slug}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Stores);
    }
}
