using Application.Abstractions.Messaging;

namespace Application.PaymentProviders.Wompi;

public sealed record GenerateWompiIntegritySignatureQuery(Guid OrderId) : IQuery<string>;
