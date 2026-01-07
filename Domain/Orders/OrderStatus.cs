namespace Domain.Orders;

public enum OrderStatus
{
    PendingPayment,
    PaymentConfirmed,
    Processing,
    ReadyForPickup,
    Shipped,
    Delivered,
    FailedDelivery,
    Canceled,
    Returned
}
