namespace Cetus.Domain;

public class OrderItem
{
    public Guid Id { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
