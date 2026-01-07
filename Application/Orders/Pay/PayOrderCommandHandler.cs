using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Pay;

internal sealed class PayOrderCommandHandler(IApplicationDbContext db, IDateTimeProvider dateTimeProvider)
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

        if (!order.CanTransitionTo(OrderStatus.PaymentConfirmed))
        {
            return Result.Failure<SimpleOrderResponse>(
                OrderErrors.InvalidStatusTransition(order.Status, OrderStatus.PaymentConfirmed));
        }

        var oldStatus = order.Status;

        order.Status = OrderStatus.PaymentConfirmed;
        order.TransactionId = command.TransactionId;
        order.PaymentProvider = command.PaymentProvider;

        order.Raise(new PaidOrderDomainEvent(new PaidOrder(
            order.Id,
            order.OrderNumber,
            order.Customer!.Name,
            order.Customer!.Email,
            order.Total
        )));

        var timelineEntry = new OrderTimeline
        {
            Id = Guid.CreateVersion7(),
            OrderId = order.Id,
            FromStatus = oldStatus,
            ToStatus = OrderStatus.PaymentConfirmed,
            CreatedAt = dateTimeProvider.UtcNow
        };

        await db.OrderTimeline.AddAsync(timelineEntry, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return SimpleOrderResponse.From(order);
    }
}
