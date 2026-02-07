using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.FindByDomain;
using Application.Stores.FindBySlug;
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
            IQueryHandler<FindStoreByDomainQuery, SimpleStoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreByDomainQuery(domain);
            string cacheKey = $"store:by-domain:${domain}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Stores);

        app.MapGet("stores/by-slug/{slug}", async (
            string slug,
            IQueryHandler<FindStoreBySlugQuery, SimpleStoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreBySlugQuery(slug);
            string cacheKey = $"store:by-slug:${slug}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Stores);
    }
}
