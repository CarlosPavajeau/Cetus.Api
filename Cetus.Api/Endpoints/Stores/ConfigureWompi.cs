using Application.Abstractions.Messaging;
using Application.Stores.ConfigureWompi;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class ConfigureWompi : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("stores/payment-providers/wompi/credentials", async (
            [FromBody] ConfigureWompiCommand command,
            ICommandHandler<ConfigureWompiCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Stores, Tags.Wompi);
    }
}
