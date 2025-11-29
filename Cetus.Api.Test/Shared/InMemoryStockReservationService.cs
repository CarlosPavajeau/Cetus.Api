using Application.Abstractions.Services;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Api.Test.Shared;

public sealed class InMemoryStockReservationService(ApplicationDbContext context) : IStockReservationService
{
    public async Task<StockReservationResult> TryReserveAsync(
        IReadOnlyDictionary<long, int> quantitiesByVariant,
        Guid storeId,
        CancellationToken cancellationToken)
    {
        if (quantitiesByVariant.Count == 0)
        {
            return new StockReservationResult(true, Array.Empty<long>(), Array.Empty<long>());
        }

        long[] ids = quantitiesByVariant.Keys.ToArray();

        var variants = await context.ProductVariants
            .Include(v => v.Product)
            .Where(v => ids.Contains(v.Id)
                        && v.DeletedAt == null
                        && v.Product != null
                        && v.Product.DeletedAt == null
                        && v.Product.StoreId == storeId)
            .ToListAsync(cancellationToken);

        var foundIds = variants.Select(v => v.Id).ToHashSet();
        var missing = ids.Where(id => !foundIds.Contains(id));
        var insufficient = variants.Where(v => v.Stock < quantitiesByVariant[v.Id]).Select(v => v.Id);

        var failed = missing.Concat(insufficient).Distinct().ToList();

        if (failed.Count != 0)
        {
            long[] reserved = ids.Where(id => !failed.Contains(id)).ToArray();
            return new StockReservationResult(false, reserved, failed);
        }

        foreach (var variant in variants)
        {
            variant.Stock -= quantitiesByVariant[variant.Id];
        }

        await context.SaveChangesAsync(cancellationToken);

        return new StockReservationResult(true, ids, []);
    }
}
