using Domain.States;
using SharedKernel;

namespace Domain.Orders;

public sealed class Order : Entity
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        { OrderStatus.PendingPayment, [OrderStatus.PaymentConfirmed, OrderStatus.Canceled] },
        { OrderStatus.PaymentConfirmed, [OrderStatus.Processing, OrderStatus.Canceled] },
        { OrderStatus.Processing, [OrderStatus.ReadyForPickup, OrderStatus.Shipped, OrderStatus.Canceled] },
        { OrderStatus.ReadyForPickup, [OrderStatus.Delivered, OrderStatus.Canceled] },
        { OrderStatus.Shipped, [OrderStatus.Delivered, OrderStatus.FailedDelivery] },
        { OrderStatus.FailedDelivery, [OrderStatus.Shipped, OrderStatus.Canceled, OrderStatus.Returned] },
        { OrderStatus.Delivered, [OrderStatus.Returned] },
        { OrderStatus.Canceled, [] },
        { OrderStatus.Returned, [] }
    };

    public Guid Id { get; set; }
    public long OrderNumber { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    public string? Address { get; set; }
    public Guid? CityId { get; set; }
    public City? City { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderChannel Channel { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentProvider? PaymentProvider { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? RefundId { get; set; }
    public DateTime? PaymentVerifiedAt { get; set; }
    public string? PaymentVerificationNotes { get; set; }

    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Guid StoreId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool CanTransitionTo(OrderStatus nextState)
    {
        // Special Business Rule: Cash on Delivery (COD)
        // In Colombia, if it is cash on delivery,
        // I can skip from Pending to Processing without the payment being confirmed yet.
        if (PaymentMethod == PaymentMethod.CashOnDelivery && Status == OrderStatus.PendingPayment &&
            nextState == OrderStatus.Processing)
        {
            return true;
        }

        bool isBasicTransitionValid = AllowedTransitions.TryGetValue(Status, out var allowedStatuses) &&
                                      allowedStatuses.Contains(nextState);

        return isBasicTransitionValid;
    }
}
