using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Pay;

internal sealed class PayOrderCommandHandler(IApplicationDbContext db)
    : ICommandHandler<PayOrderCommand, SimpleOrderResponse>
{
    public async Task<Result<SimpleOrderResponse>> Handle(PayOrderCommand command, CancellationToken cancellationToken)
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
                OrderErrors.InvalidStatusTransition(order.Status, OrderStatus.Paid));
        }

        order.Status = OrderStatus.Paid;
        order.TransactionId = command.TransactionId;

        order.Raise(new PaidOrderDomainEvent(new PaidOrder(
            order.Id,
            order.OrderNumber,
            order.Customer!.Name,
            order.Customer!.Email,
            order.Total
        )));

        await db.SaveChangesAsync(cancellationToken);

        return SimpleOrderResponse.From(order);
    }
}
