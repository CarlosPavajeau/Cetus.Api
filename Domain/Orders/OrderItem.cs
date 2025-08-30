using Domain.Products;

namespace Domain.Orders;

public sealed class OrderItem
{
    public Guid Id { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public Guid ProductId => ProductVariant!.ProductId;
    public Product? Product => ProductVariant!.Product;

    public long VariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
