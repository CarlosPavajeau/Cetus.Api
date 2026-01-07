using Domain.Orders;

namespace Application.Orders.SearchPayment;

public record OrderPaymentResponse(
    PaymentProvider PaymentProvider,
    string TransactionId,
    string Status,
    PaymentMethod PaymentMethod,
    DateTime? CreatedAt,
    DateTime? ApprovedAt
);
