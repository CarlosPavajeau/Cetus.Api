namespace Domain.Products;

public sealed class ProductImage
{
    public long Id { get; set; }
    public Guid ProductId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }

    public long? VariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public DateTime CreatedAt { get; set; }
}
