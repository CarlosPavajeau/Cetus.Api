using Application.Abstractions.Messaging;
using Application.States.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.States;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("states", async (
            IQueryHandler<SearchAllStatesQuery, IEnumerable<StateResponse>> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllStatesQuery();

            var result = await cache.GetOrCreateAsync(
                "states",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.States);
    }
}
