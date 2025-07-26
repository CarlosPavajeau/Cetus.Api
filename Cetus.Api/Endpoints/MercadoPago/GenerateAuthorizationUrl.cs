using Application.Abstractions.MercadoPago;

namespace Cetus.Api.Endpoints.MercadoPago;

internal sealed class GenerateAuthorizationUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("mercadopago/authorization-url", async (
            IMercadoPagoClient mercadoPagoClient,
            CancellationToken cancellationToken
        ) =>
        {
            var authorizationUrl = await mercadoPagoClient.GenerateAuthorizationUrl(cancellationToken);
            return Results.Ok(authorizationUrl);
        }).WithTags(Tags.MercadoPago);
    }
}
