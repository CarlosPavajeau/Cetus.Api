using Application.Abstractions.Messaging;
using Application.States.FindCity;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.States;

internal sealed class FindCity : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/states/cities/{id:guid}", async (
            Guid id,
            IQueryHandler<FindCityQuery, CityResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindCityQuery(id);

            var result = await cache.GetOrCreateAsync(
                $"states-cities-{id}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(2),
                    LocalCacheExpiration = TimeSpan.FromHours(2)
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.States);
    }
}
