using Application.Abstractions.Services;
using Domain.Products;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Products;

public sealed class StockReservationService(ApplicationDbContext context) : IStockReservationService
{
    private sealed record UpdatedStockResult(long Id, int NewStock);

    public async Task<StockReservationResult> TryReserveAsync(
        IReadOnlyDictionary<long, int> quantitiesByVariant,
        Guid orderId,
        Guid storeId,
        CancellationToken cancellationToken)
    {
        if (quantitiesByVariant.Count == 0)
        {
            return new StockReservationResult(true, [], []);
        }

        long[] ids = [.. quantitiesByVariant.Keys];
        int[] qtys = [.. quantitiesByVariant.Values];

        const string sql = """
                           WITH input_data AS (
                               SELECT UNNEST(@ids)::bigint AS id, UNNEST(@qtys)::int AS qty
                           ),
                           updated_rows AS (
                               UPDATE product_variants pv
                               SET stock = pv.stock - v.qty
                               FROM input_data v
                               WHERE pv.id = v.id
                                 AND pv.deleted_at IS NULL
                                 AND pv.stock >= v.qty  -- Constraint de negocio
                                 AND EXISTS (
                                     SELECT 1 FROM products p
                                     WHERE p.id = pv.product_id
                                       AND p.deleted_at IS NULL
                                       AND p.store_id = @store_id
                                 )
                               RETURNING pv.id, pv.stock as "NewStock"
                           )
                           SELECT id, "NewStock" FROM updated_rows;
                           """;

        var idsParam = new NpgsqlParameter<long[]>("@ids", ids);
        var qtysParam = new NpgsqlParameter<int[]>("@qtys", qtys);
        var storeParam = new NpgsqlParameter<Guid>("@store_id", storeId);

        object[] parameters = [idsParam, qtysParam, storeParam];

        var successfulUpdates = await context.Database
            .SqlQueryRaw<UpdatedStockResult>(sql, parameters)
            .ToListAsync(cancellationToken);

        var reservedIds = successfulUpdates.Select(x => x.Id).ToHashSet();
        long[] failedIds = ids.Where(id => !reservedIds.Contains(id)).ToArray();

        bool isSuccess = failedIds.Length == 0;

        if (!isSuccess)
        {
            return new StockReservationResult(false, reservedIds.ToArray(), failedIds);
        }

        var transactions = successfulUpdates.Select(update => new InventoryTransaction
        {
            VariantId = update.Id,
            Type = InventoryTransactionType.Sale,
            Quantity = -quantitiesByVariant[update.Id],
            StockAfter = update.NewStock,
            ReferenceId = orderId.ToString(),
            Reason = "Order Reservation",
            CreatedAt = DateTime.UtcNow
        });

        await context.InventoryTransactions.AddRangeAsync(transactions, cancellationToken);

        return new StockReservationResult(true, ids, []);
    }
}
