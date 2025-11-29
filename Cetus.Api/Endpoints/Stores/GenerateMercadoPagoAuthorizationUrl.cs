using Application.Abstractions.MercadoPago;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class GenerateMercadoPagoAuthorizationUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stores/payment-providers/mercado-pago/authorization-url", async (
            IMercadoPagoClient mercadoPagoClient,
            CancellationToken cancellationToken
        ) =>
        {
            string? authorizationUrl = await mercadoPagoClient.GenerateAuthorizationUrl(cancellationToken);
            return Results.Ok(authorizationUrl);
        }).WithTags(Tags.Stores, Tags.MercadoPago);
    }
}
