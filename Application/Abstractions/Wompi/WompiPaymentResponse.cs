using Domain.Orders;

namespace Application.Abstractions.Wompi;

public sealed record WompiPaymentResponse(
    string TransactionId,
    string Status,
    string PaymentMethodType,
    PaymentMethod PaymentMethod,
    DateTime? CreatedAt,
    DateTime? ApprovedAt
);
