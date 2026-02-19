using Application.Abstractions.Messaging;
using Application.Stores.ConfigureMercadoPago;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class ConfigureMercadoPago : IEndpoint
{
    private sealed record Request(string AccessToken, string RefreshToken, long ExpiresIn);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("stores/payment-providers/mercado-pago/credentials", async (
            Request request,
            ICommandHandler<ConfigureMercadoPagoCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfigureMercadoPagoCommand(request.AccessToken, request.RefreshToken, request.ExpiresIn);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Stores);
    }
}
