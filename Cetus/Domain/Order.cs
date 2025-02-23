namespace Cetus.Domain;

public class Order
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public IEnumerable<OrderItem> Items { get; set; } = new List<OrderItem>();

    public string CustomerId { get; set; } = string.Empty;
    public Customer? Customer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
