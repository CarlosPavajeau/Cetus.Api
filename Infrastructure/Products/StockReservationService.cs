using Application.Abstractions.Services;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Products;

public sealed class StockReservationService(ApplicationDbContext context) : IStockReservationService
{
    public async Task<StockReservationResult> TryReserveAsync(
        IReadOnlyDictionary<long, int> quantitiesByVariant,
        Guid storeId,
        CancellationToken cancellationToken)
    {
        if (quantitiesByVariant.Count == 0)
        {
            return new StockReservationResult(true, [], []);
        }

        var ids = quantitiesByVariant.Keys.ToArray();
        var qtys = quantitiesByVariant.Values.ToArray();

        const string sql = """
                           UPDATE product_variants pv
                           SET stock = pv.stock - v.qty
                           FROM (SELECT UNNEST(@ids)::bigint AS id, UNNEST(@qtys)::int AS qty) v
                           WHERE pv.id = v.id
                             AND pv.deleted_at IS NULL
                             AND pv.stock >= v.qty
                             AND EXISTS (
                                 SELECT 1 FROM products p
                                 WHERE p.id = pv.product_id
                                   AND p.deleted_at IS NULL
                                   AND p.store_id = @store_id
                             );
                           """;

        var idsParam = new NpgsqlParameter<long[]>("@ids", ids);
        var qtysParam = new NpgsqlParameter<int[]>("@qtys", qtys);
        var storeParam = new NpgsqlParameter<Guid>("@store_id", storeId);

        var affected = await context.Database.ExecuteSqlRawAsync(sql, idsParam, qtysParam, storeParam);

        if (affected == quantitiesByVariant.Count)
        {
            return new StockReservationResult(true, ids, []);
        }

        // Re-check to know which failed by selecting those that cannot fulfill the qty
        var failedIds = await context.ProductVariants
            .Where(v => ids.Contains(v.Id))
            .Select(v => new {v.Id, v.Stock, v.Product!.DeletedAt, v.Product.StoreId})
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var failed = failedIds
            .Where(x => x.DeletedAt != null || x.StoreId != storeId || x.Stock < quantitiesByVariant[x.Id])
            .Select(x => x.Id)
            .ToList();

        var reserved = ids.Where(id => !failed.Contains(id)).ToArray();

        return new StockReservationResult(false, reserved, failed);
    }
}
