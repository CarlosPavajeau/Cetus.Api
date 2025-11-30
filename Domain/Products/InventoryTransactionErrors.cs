using SharedKernel;

namespace Domain.Products;

public static class InventoryTransactionErrors
{
    public static Error CannotAdjustInventoryStock(string reason) =>
        Error.Failure("InventoryTransaction.CannotAdjustInventoryStock", reason);

    public static Error NegativeStockNotAllowed(long variantId, int resultingStock) =>
        Error.Failure(
            "InventoryTransaction.NegativeStockNotAllowed",
            $"Adjustment would result in negative stock ({resultingStock}) for variant ID {variantId}."
        );
}
