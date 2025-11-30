namespace Domain.Products;

public class InventoryTransaction
{
    public Guid Id { get; set; }
    public long VariantId { get; set; }
    public InventoryTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int StockAfter { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceId { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ProductVariant? ProductVariant { get; set; }
}
