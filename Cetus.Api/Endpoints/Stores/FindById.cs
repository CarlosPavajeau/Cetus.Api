using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.FindById;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class FindById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stores/{id:guid}", async (
            Guid id,
            IQueryHandler<FindStoreByIdQuery, SimpleStoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreByIdQuery(id);
            var cacheKey = $"store-id-{id}";

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
        }).AllowAnonymous().WithTags(Tags.Stores);
    }
}
