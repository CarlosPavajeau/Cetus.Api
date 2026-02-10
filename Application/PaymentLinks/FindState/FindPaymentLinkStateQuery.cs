using Application.Abstractions.Messaging;

namespace Application.PaymentLinks.FindState;

public sealed record FindPaymentLinkStateQuery(Guid OrderId) : IQuery<PaymentLinkStateResponse>;
