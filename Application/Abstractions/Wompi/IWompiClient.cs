using Application.Orders.SearchPayment;

namespace Application.Abstractions.Wompi;

public interface IWompiClient
{
    Task<OrderPaymentResponse?> FindPaymentById(string paymentId, string publicKey, CancellationToken cancellationToken = default);
}
