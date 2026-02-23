namespace Application.Abstractions.MercadoPago;

public sealed record MercadoPagoTokenResponse(string AccessToken, string RefreshToken, long ExpiresIn);
