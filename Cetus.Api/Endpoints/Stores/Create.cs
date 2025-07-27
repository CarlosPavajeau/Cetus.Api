using Application.Abstractions.Messaging;
using Application.Stores.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("stores", async (
            [FromBody] CreateStoreCommand command,
            ICommandHandler<CreateStoreCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
            
        }).WithTags(Tags.Stores);
    }
}
