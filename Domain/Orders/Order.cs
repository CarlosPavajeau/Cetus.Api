using Domain.States;
using SharedKernel;

namespace Domain.Orders;

public sealed class Order : Entity
{
    public Guid Id { get; set; }
    public long OrderNumber { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string Address { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public City? City { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

    public string CustomerId { get; set; } = string.Empty;
    public Customer? Customer { get; set; }

    public string? TransactionId { get; set; }
    public string? RefundId { get; set; }

    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Guid StoreId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
