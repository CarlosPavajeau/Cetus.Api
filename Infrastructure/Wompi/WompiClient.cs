using System.Net.Http.Json;
using Application.Abstractions.Wompi;
using Application.Orders.SearchPayment;
using Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Wompi;

internal sealed class WompiClient(IHttpClientFactory clientFactory, ILogger<WompiClient> logger) : IWompiClient
{
    public async Task<OrderPaymentResponse?> FindPaymentById(string paymentId, string publicKey,
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

            return new OrderPaymentResponse(
                PaymentProvider.Wompi,
                transaction.Data.Id,
                transaction.Data.Status,
                transaction.Data.PaymentMethodType,
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
}
