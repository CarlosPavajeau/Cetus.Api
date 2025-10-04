namespace Domain.Products;

public sealed class ProductOption
{
    public Guid ProductId { get; set; }
    public long OptionTypeId { get; set; }

    public Product? Product { get; set; }
    public ProductOptionType? ProductOptionType { get; set; }
}
