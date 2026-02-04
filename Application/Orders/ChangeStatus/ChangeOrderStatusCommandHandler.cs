using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Cancel;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.ChangeStatus;

internal sealed class ChangeOrderStatusCommandHandler(
    IApplicationDbContext db,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ChangeOrderStatusCommand>
{
    public async Task<Result> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Customer)
            .Where(o => o.Id == command.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(command.OrderId));
        }

        var oldStatus = order.Status;

        if (command is { PaymentMethod: not null, NewStatus: OrderStatus.PaymentConfirmed })
        {
            order.PaymentMethod = command.PaymentMethod.Value;
            order.PaymentProvider = PaymentProvider.Manual;
            order.PaymentStatus = PaymentStatus.Verified;
        }

        if (command.PaymentStatus.HasValue)
        {
            order.PaymentStatus = command.PaymentStatus.Value;
        }

        if (!order.CanTransitionTo(command.NewStatus))
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition(oldStatus, command.NewStatus));
        }

        order.Status = command.NewStatus;

        switch (order.Status)
        {
            case OrderStatus.Canceled:
                order.CancellationReason = command.Notes;
                order.CancelledAt = dateTimeProvider.UtcNow;

                order.Raise(new CanceledOrderDomainEvent(order.Id));
                break;
            case OrderStatus.Shipped:
                order.Raise(new SentOrderDomainEvent(new SentOrder(
                    order.Id,
                    order.OrderNumber,
                    order.Customer?.Name,
                    order.Address,
                    order.Customer?.Email
                )));
                break;
            case OrderStatus.Delivered:
                order.Raise(new DeliveredOrderDomainEvent(order.Id));
                break;
        }

        var timelineEntry = new OrderTimeline
        {
            Id = Guid.CreateVersion7(),
            OrderId = order.Id,
            FromStatus = oldStatus,
            ToStatus = command.NewStatus,
            ChangedById = command.UserId,
            Notes = command.Notes,
            CreatedAt = dateTimeProvider.UtcNow
        };

        await db.OrderTimeline.AddAsync(timelineEntry, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
