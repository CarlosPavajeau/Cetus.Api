using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Products.Inventory.Adjust;

internal sealed class AdjustInventoryStockCommandHandler(
    IApplicationDbContext db,
    IDateTimeProvider dateTimeProvider,
    ILogger<AdjustInventoryStockCommandHandler> logger) : ICommandHandler<AdjustInventoryStockCommand>
{
    public async Task<Result> Handle(AdjustInventoryStockCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        try
        {
            var variantIds = command.Adjustments.Select(x => x.VariantId).ToList();
            var variants = await db.ProductVariants
                .Where(x => variantIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
            var variantsById = variants.ToDictionary(x => x.Id);

            foreach (var adjustment in command.Adjustments)
            {
                if (!variantsById.TryGetValue(adjustment.VariantId, out var variant))
                {
                    logger.LogWarning("Product variant with ID {VariantId} not found", adjustment.VariantId);
                    await transaction.RollbackAsync(cancellationToken);

                    return Result.Failure(ProductVariantErrors.NotFound(adjustment.VariantId));
                }

                int previousStock = variant.Stock;
                int newStock;
                int quantityChange;

                if (adjustment.Type == AdjustmentType.Delta)
                {
                    quantityChange = adjustment.Value;
                    newStock = previousStock + quantityChange;
                }
                else
                {
                    newStock = adjustment.Value;
                    quantityChange = newStock - previousStock;
                }

                if (newStock < 0)
                {
                    logger.LogWarning("Adjustment would result in negative stock for variant {VariantId}: {NewStock}",
                        variant.Id, newStock);

                    await transaction.RollbackAsync(cancellationToken);

                    return Result.Failure(InventoryTransactionErrors.NegativeStockNotAllowed(variant.Id, newStock));
                }

                variant.Stock = newStock;

                if (quantityChange == 0)
                {
                    logger.LogInformation("Skipping transaction record for variant {VariantId} - no stock change",
                        variant.Id);

                    continue;
                }

                string reason = string.Join(" - ", new[]
                {
                    command.GlobalReason,
                    string.IsNullOrWhiteSpace(adjustment.Reason) ? null : adjustment.Reason
                }.Where(x => !string.IsNullOrWhiteSpace(x)));

                var inventoryTransaction = new InventoryTransaction
                {
                    Id = Guid.CreateVersion7(),
                    VariantId = variant.Id,
                    Type = InventoryTransactionType.Adjustment,
                    Quantity = quantityChange,
                    StockAfter = newStock,
                    Reason = string.IsNullOrWhiteSpace(reason) ? null : reason,
                    UserId = command.UserId,
                    CreatedAt = dateTimeProvider.UtcNow
                };

                await db.InventoryTransactions.AddAsync(inventoryTransaction, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adjusting inventory stock");
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure(
                InventoryTransactionErrors.CannotAdjustInventoryStock(
                    "An unexpected error occurred while adjusting inventory stock"));
        }
    }
}
