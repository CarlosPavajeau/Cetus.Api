namespace Application.Abstractions.Wompi;

public sealed record WompiPaymentResponse(
    string TransactionId,
    string Status,
    string PaymentMethod,
    DateTime? CreatedAt,
    DateTime? ApprovedAt
);
