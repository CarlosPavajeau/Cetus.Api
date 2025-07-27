using MercadoPago.Resource.Payment;

namespace Application.Abstractions.MercadoPago;

public interface IMercadoPagoClient
{
    Task<string?> GenerateAuthorizationUrl(CancellationToken cancellationToken = default);
    Task<Payment?> FindPaymentById(long paymentId, CancellationToken cancellationToken = default);
}
