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

        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var orderItem = order.Items.First(i => i.ProductId == product.Id);
            product.SalesCount += orderItem.Quantity;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated sales count for products in order {OrderId}", domainEvent.Order.Id);
    }
} 
