using System.Net.Http.Json;
using Application.Abstractions.Wompi;
using Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Wompi;

internal sealed class WompiClient(IHttpClientFactory clientFactory, ILogger<WompiClient> logger) : IWompiClient
{
    private static readonly Dictionary<string, PaymentMethod> PaymentMethods = new()
    {
        { "BANCOLOMBIA_TRANSFER", PaymentMethod.BankTransfer },
        { "PSE", PaymentMethod.PSE },
        { "NEQUI", PaymentMethod.BankTransfer },
        { "CARD", PaymentMethod.CreditCard }
    };

    public async Task<WompiPaymentResponse?> FindPaymentById(string paymentId,
        CancellationToken cancellationToken = default)
    {
        var client = clientFactory.CreateClient(nameof(WompiClient));

        try
        {
            var transaction =
                await client.GetFromJsonAsync<Transaction>($"v1/transactions/{paymentId}", cancellationToken);

            if (transaction is null)
            {
                return null;
            }

            var paymentMethod = GetPaymentMethodFromWompi(transaction.Data.PaymentMethodType);

            return new WompiPaymentResponse(
                transaction.Data.Id,
                transaction.Data.Status,
                transaction.Data.PaymentMethodType,
                paymentMethod,
                transaction.Data.CreatedAt,
                transaction.Data.FinalizedAt
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error searching Wompi payment with id {PaymentId}", paymentId);
            return null;
        }
    }

    private static PaymentMethod GetPaymentMethodFromWompi(string wompiPaymentMethod)
    {
        return PaymentMethods.TryGetValue(wompiPaymentMethod, out var paymentMethod)
            ? paymentMethod
            : PaymentMethod.Cash;
    }
}
