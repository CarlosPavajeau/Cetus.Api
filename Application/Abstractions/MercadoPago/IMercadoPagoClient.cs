using MercadoPago.Client.Preference;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using PaymentMethod = Domain.Orders.PaymentMethod;

namespace Application.Abstractions.MercadoPago;

public interface IMercadoPagoClient
{
    Task<string?> GenerateAuthorizationUrl(CancellationToken cancellationToken = default);
    Task<Payment?> FindPaymentById(long paymentId, CancellationToken cancellationToken = default);

    Task<Preference?> CreatePreference(PreferenceRequest request, string accessToken,
        CancellationToken cancellationToken = default);

    Task<Payment?> CancelPayment(long paymentId, string accessToken, CancellationToken cancellationToken = default);

    Task<PaymentRefund?> RefundPayment(long paymentId, string accessToken,
        CancellationToken cancellationToken = default);

    PaymentMethod GetPaymentMethodFromMercadoPago(string mercadoPagoPaymentMethod);
}
