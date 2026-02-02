namespace Domain.Orders;

public enum PaymentStatus
{
    Pending,
    AwaitingVerification,
    Verified,
    Rejected,
    Refunded
}
