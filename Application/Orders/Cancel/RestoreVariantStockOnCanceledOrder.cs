using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Cancel;

internal sealed class RestoreVariantStockOnCanceledOrder(
    IApplicationDbContext db,
    ILogger<RestoreVariantStockOnCanceledOrder> logger)
    : IDomainEventHandler<CanceledOrderDomainEvent>
{
    public async Task Handle(CanceledOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Restoring variant stock on canceled order");

        var items = await db.OrderItems
            .AsNoTracking()
            .Where(o => o.OrderId == domainEvent.OrderId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return;
        }

        var variantIds = items.Select(i => i.VariantId).Distinct().ToList();
        var variants = await db.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync(cancellationToken);

        var quantityByVariantId = items
            .GroupBy(i => i.VariantId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        foreach (var variant in variants)
        {
            if (quantityByVariantId.TryGetValue(variant.Id, out var quantity))
            {
                variant.Stock += quantity;
                logger.LogInformation("Restored {Quantity} units to variant {VariantId}. New stock: {NewStock}",
                    quantity, variant.Id, variant.Stock);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
