namespace Domain.Products;

public sealed class ProductOptionValue
{
    public long Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public long OptionTypeId { get; set; }

    public ProductOptionType? ProductOptionType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
