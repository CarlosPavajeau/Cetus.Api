namespace Domain.Products;

public sealed class ProductVariantOptionValue
{
    public long VariantId { get; set; }
    public long OptionValueId { get; set; }

    public ProductVariant? ProductVariant { get; set; }
    public ProductOptionValue? ProductOptionValue { get; set; }
}
