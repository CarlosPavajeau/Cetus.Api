using Application.Abstractions.Messaging;

namespace Application.PaymentProviders.MercadoPago;

public sealed record CreateMercadoPagoPreferenceCommand(Guid OrderId) : ICommand<string>;
