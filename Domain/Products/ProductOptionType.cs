namespace Domain.Products;

public sealed class ProductOptionType
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductOptionValue> ProductOptionValues { get; set; } = new HashSet<ProductOptionValue>();

    public Guid StoreId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
