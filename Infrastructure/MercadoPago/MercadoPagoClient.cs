using Application.Abstractions.MercadoPago;
using MercadoPago.Client.OAuth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.MercadoPago;

public class MercadoPagoClient(IConfiguration configuration) : IMercadoPagoClient
{
    public async Task<string?> GenerateAuthorizationUrl(CancellationToken cancellationToken = default)
    {
        var clientId = configuration["MercadoPago:ClientId"];

        if (string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException("MercadoPago clientId is missing");
        }

        var redirectUri = configuration["MercadoPago:RedirectUri"];

        if (string.IsNullOrEmpty(redirectUri))
        {
            throw new InvalidOperationException("MercadoPago redirect URI is missing");
        }

        var oAuthClient = new OAuthClient();

        var authorizationUrl = await oAuthClient.GetAuthorizationURLAsync(
            clientId,
            redirectUri,
            cancellationToken: cancellationToken
        );

        return authorizationUrl;
    }
}
