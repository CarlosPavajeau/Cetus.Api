using Application.Abstractions.Data;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Update;

internal sealed class UpdateProductSalesCountDomainEventHandler(
    IApplicationDbContext context,
    ILogger<UpdateProductSalesCountDomainEventHandler> logger)
    : IDomainEventHandler<SentOrderDomainEvent>
{
    public async Task Handle(SentOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating sales count for products in order {OrderId}", domainEvent.Order.Id);

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == domainEvent.Order.Id, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when updating sales count", domainEvent.Order.Id);
            return;
        }

        var variantIds = order.Items.Select(i => i.VariantId).ToList();
        var variants = await context.ProductVariants
            .Where(p => variantIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var variant in variants)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.VariantId == variant.Id);

            if (orderItem is null)
            {
                logger.LogWarning("Order item not found for variant {VariantId} in order {OrderId}", variant.Id,
                    order.Id);
                continue;
            }

            variant.SalesCount += orderItem.Quantity;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated sales count for products in order {OrderId}", domainEvent.Order.Id);
    }
}
