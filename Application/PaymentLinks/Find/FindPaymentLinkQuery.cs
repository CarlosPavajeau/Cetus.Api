using Application.Abstractions.Messaging;

namespace Application.PaymentLinks.Find;

public sealed record FindPaymentLinkQuery(string Token) : IQuery<PaymentLinkResponse>;
