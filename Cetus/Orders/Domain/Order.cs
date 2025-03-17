using System.ComponentModel.DataAnnotations;
using Cetus.States.Domain;

namespace Cetus.Orders.Domain;

public class Order
{
    [Required] [Key] public Guid Id { get; set; }
    public long OrderNumber { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required] [MaxLength(256)] public string Address { get; set; } = string.Empty;

    public Guid? CityId { get; set; }
    public City? City { get; set; }

    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }

    public IEnumerable<OrderItem> Items { get; set; } = new List<OrderItem>();

    [Required] [MaxLength(50)] public string CustomerId { get; set; } = string.Empty;

    public Customer? Customer { get; set; }

    [MaxLength(256)] public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
