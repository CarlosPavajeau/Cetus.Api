using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Deliver;

internal sealed class DeliverOrderCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeliverOrderCommand, SimpleOrderResponse>
{
    public async Task<Result<SimpleOrderResponse>> Handle(DeliverOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Customer)
            .Where(o => o.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<SimpleOrderResponse>(OrderErrors.NotFound(command.Id));
        }

        if (order.Status == OrderStatus.Canceled) // Can't update a canceled order
        {
            return Result.Failure<SimpleOrderResponse>(
                OrderErrors.InvalidStatusTransition(order.Status, OrderStatus.Delivered));
        }

        order.Status = OrderStatus.Delivered;

        var customer = order.Customer!;

        order.Raise(new SentOrderDomainEvent(new SentOrder(
            order.Id,
            order.OrderNumber,
            customer.Name,
            customer.Address,
            customer.Email
        )));

        order.Raise(new DeliveredOrderDomainEvent(order.Id));

        await db.SaveChangesAsync(cancellationToken);

        return SimpleOrderResponse.From(order);
    }
}
