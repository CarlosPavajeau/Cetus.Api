using Application.Abstractions.Messaging;
using Application.States.SearchAllCities;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.States;

internal sealed class SearchAllStateCities : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("states/{id:guid}/cities", async (
            Guid id,
            IQueryHandler<SearchAllStateCitiesQuery, IEnumerable<CityResponse>> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllStateCitiesQuery(id);

            var result = await cache.GetOrCreateAsync(
                $"states-{id}-cities",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.States);
    }
}
