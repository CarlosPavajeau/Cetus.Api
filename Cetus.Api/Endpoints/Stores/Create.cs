using Application.Abstractions.Messaging;
using Application.Stores.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class Create : IEndpoint
{
    private sealed record Request(string Name, string Slug, string ExternalId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("stores", async (
            Request request,
            ICommandHandler<CreateStoreCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateStoreCommand(request.Name, request.Slug, request.ExternalId);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Stores);
    }
}
