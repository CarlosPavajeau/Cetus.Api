using Application.Abstractions.Messaging;
using Application.Stores.ConfigureMercadoPago;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class ConfigureMercadoPago : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("stores/payment-providers/mercado-pago/credentials", async (
            [FromBody] ConfigureMercadoPagoCommand command,
            ICommandHandler<ConfigureMercadoPagoCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Stores);
    }
}
