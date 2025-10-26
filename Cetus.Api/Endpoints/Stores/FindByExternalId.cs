using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.FindByExternalId;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class FindByExternalId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stores/by-external-id/{externalId}", async (
            string externalId,
            IQueryHandler<FindStoreByExternalId, StoreResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreByExternalId(externalId);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Stores);
    }
}
