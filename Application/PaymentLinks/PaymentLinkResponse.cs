using Domain.PaymentLinks;

namespace Application.PaymentLinks;

public sealed record PaymentLinkResponse(
    Guid Id,
    Guid OrderId,
    string Token,
    string Url,
    PaymentLinkStatus Status,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    int TimeRemaining
);
