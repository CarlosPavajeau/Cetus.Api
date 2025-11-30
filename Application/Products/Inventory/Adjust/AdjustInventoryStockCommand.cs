using Application.Abstractions.Messaging;

namespace Application.Products.Inventory.Adjust;

public sealed record AdjustInventoryStockCommand(
    string? GlobalReason,
    string UserId,
    IReadOnlyList<InventoryAdjustmentItem> Adjustments
) : ICommand;

public sealed record InventoryAdjustmentItem(
    long VariantId,
    int Value,
    AdjustmentType Type,
    string? Reason
);

public enum AdjustmentType
{
    Delta, // Add/Subtract from current stock (e.g. "Add 5 to stock")
    Snapshot // Overwrite current stock (e.g. "Set stock to 10")
}
