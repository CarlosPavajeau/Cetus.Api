using System.Linq.Expressions;
using Application.Products.Variants;
using Domain.Products;

namespace Application.Products.Inventory.Transactions;

public record InventoryTransactionResponse(
    Guid Id,
    DateTime CreatedAt,
    string ProductName,
    string Sku,
    long VariantId,
    IReadOnlyList<VariantOptionValueResponse> OptionValues,
    InventoryTransactionType Type,
    int Quantity,
    int StockAfter,
    string? Reason,
    string? ReferenceId
)
{
    public static Expression<Func<InventoryTransaction, InventoryTransactionResponse>> Map => transaction =>
        new InventoryTransactionResponse(
            transaction.Id,
            transaction.CreatedAt,
            transaction.ProductVariant!.Product!.Name,
            transaction.ProductVariant.Sku,
            transaction.VariantId,
            transaction.ProductVariant.OptionValues
                .Select(ov => new VariantOptionValueResponse(
                    ov.OptionValueId,
                    ov.ProductOptionValue!.Value,
                    ov.ProductOptionValue.OptionTypeId,
                    ov.ProductOptionValue.ProductOptionType!.Name
                ))
                .ToList(),
            transaction.Type,
            transaction.Quantity,
            transaction.StockAfter,
            transaction.Reason,
            transaction.ReferenceId
        );
}
