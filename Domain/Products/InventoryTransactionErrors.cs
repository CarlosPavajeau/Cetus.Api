using SharedKernel;

namespace Domain.Products;

public static class InventoryTransactionErrors
{
    public static Error CannotAdjustInventoryStock(string reason) =>
        Error.Failure("InventoryTransaction.CannotAdjustInventoryStock", reason);
}
