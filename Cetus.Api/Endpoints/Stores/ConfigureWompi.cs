using Application.Abstractions.Messaging;
using Application.Stores.ConfigureWompi;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class ConfigureWompi : IEndpoint
{
    private sealed record Request(string PublicKey, string PrivateKey, string EventsKey, string IntegrityKey);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("stores/payment-providers/wompi/credentials", async (
            Request request,
            ICommandHandler<ConfigureWompiCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfigureWompiCommand(request.PublicKey, request.PrivateKey, request.EventsKey,
                request.IntegrityKey);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Stores, Tags.Wompi);
    }
}
