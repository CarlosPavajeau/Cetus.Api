namespace Application.Abstractions.Wompi;

public interface IWompiClient
{
    Task<WompiPaymentResponse?> FindPaymentById(string paymentId, CancellationToken cancellationToken = default);
}
