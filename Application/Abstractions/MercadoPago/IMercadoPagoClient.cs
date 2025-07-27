using MercadoPago.Client;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;

namespace Application.Abstractions.MercadoPago;

public interface IMercadoPagoClient
{
    Task<string?> GenerateAuthorizationUrl(CancellationToken cancellationToken = default);
    Task<Payment?> FindPaymentById(long paymentId, CancellationToken cancellationToken = default);
    Task<Preference?> CreatePreference(PreferenceRequest request, string accessToken, CancellationToken cancellationToken = default);
}
