namespace Application.PaymentLinks.FindState;

public sealed record PaymentLinkStateResponse(
    bool CanGenerateLink,
    string? Reason,
    PaymentLinkResponse? ActiveLink
);
