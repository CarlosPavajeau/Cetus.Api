using Application.Abstractions.Messaging;

namespace Application.PaymentLinks.Create;

public sealed record CreatePaymentLinkCommand(Guid OrderId, int ExpirationHours = 24) : ICommand<PaymentLinkResponse>;
