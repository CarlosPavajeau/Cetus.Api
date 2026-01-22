using Application.Abstractions.MercadoPago;
using MercadoPago.Client;
using MercadoPago.Client.OAuth;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentMethod = Domain.Orders.PaymentMethod;

namespace Infrastructure.MercadoPago;

public class MercadoPagoClient(IConfiguration configuration, ILogger<MercadoPagoClient> logger) : IMercadoPagoClient
{
    private static readonly Dictionary<string, PaymentMethod> PaymentMethods = new()
    {
        { "ticket", PaymentMethod.CashReference },
        { "cash", PaymentMethod.Cash },
        { "bank_transfer", PaymentMethod.BankTransfer },
        { "credit_card", PaymentMethod.CreditCard },
        { "debit_card", PaymentMethod.CreditCard },
        { "digital_wallet", PaymentMethod.BankTransfer },
        { "account_money", PaymentMethod.BankTransfer }
    };

    public async Task<string?> GenerateAuthorizationUrl(CancellationToken cancellationToken = default)
    {
        string? clientId = configuration["MercadoPago:ClientId"];

        if (string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException("MercadoPago clientId is missing");
        }

        string? redirectUri = configuration["MercadoPago:RedirectUri"];

        if (string.IsNullOrEmpty(redirectUri))
        {
            throw new InvalidOperationException("MercadoPago redirect URI is missing");
        }

        var oAuthClient = new OAuthClient();

        string? authorizationUrl = await oAuthClient.GetAuthorizationURLAsync(
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

    public async Task<Preference?> CreatePreference(PreferenceRequest request, string accessToken,
        CancellationToken cancellationToken = default)
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

    public async Task<Payment?> CancelPayment(long paymentId, string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentClient = new PaymentClient();
            var options = new RequestOptions
            {
                AccessToken = accessToken
            };

            var result = await paymentClient.CancelAsync(paymentId, options, cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error cancelling MercadoPago payment for paymentId {PaymentId}", paymentId);
            return null;
        }
    }

    public async Task<PaymentRefund?> RefundPayment(long paymentId, string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentClient = new PaymentClient();
            var options = new RequestOptions
            {
                AccessToken = accessToken
            };

            var result = await paymentClient.RefundAsync(paymentId, options, cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error refunding MercadoPago payment for paymentId {PaymentId}", paymentId);
            return null;
        }
    }

    public PaymentMethod GetPaymentMethodFromMercadoPago(string mercadoPagoPaymentMethod)
    {
        return PaymentMethods.TryGetValue(mercadoPagoPaymentMethod, out var paymentMethod)
            ? paymentMethod
            : PaymentMethod.Cash;
    }
}
