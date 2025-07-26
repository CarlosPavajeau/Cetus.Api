namespace Application.Abstractions.MercadoPago;

public interface IMercadoPagoClient
{
    Task<string?> GenerateAuthorizationUrl(CancellationToken cancellationToken = default);
}
