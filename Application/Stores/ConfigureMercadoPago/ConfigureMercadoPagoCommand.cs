using Application.Abstractions.Messaging;

namespace Application.Stores.ConfigureMercadoPago;

public sealed record ConfigureMercadoPagoCommand(string AccessToken, string RefreshToken, long ExpiresIn) : ICommand;
