using Application.Abstractions.MercadoPago;
using MercadoPago.Client;
using MercadoPago.Client.OAuth;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.MercadoPago;

public class MercadoPagoClient(IConfiguration configuration, ILogger<MercadoPagoClient> logger) : IMercadoPagoClient
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

    public async Task<Payment?> FindPaymentById(long paymentId, CancellationToken cancellationToken = default)
    {
        var paymentClient = new PaymentClient();
        
        var payment = await paymentClient.GetAsync(paymentId, cancellationToken: cancellationToken);

        return payment;
    }

    public async Task<Preference?> CreatePreference(PreferenceRequest request, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferenceClient = new PreferenceClient();
            var options = new RequestOptions
            {
                AccessToken = accessToken
            };

            var preference = await preferenceClient.CreateAsync(request, options, cancellationToken: cancellationToken);

            return preference;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating MercadoPago preference");
            return null;
        }
    }
}
