namespace Domain.Products;

public sealed class ProductVariant
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new HashSet<ProductImage>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
