using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.ChangeStatus;

internal sealed class ChangeOrderStatusCommandHandler(
    IApplicationDbContext db,
    IDateTimeProvider dateTimeProvider,
    ILogger<ChangeOrderStatusCommandHandler> logger)
    : ICommandHandler<ChangeOrderStatusCommand>
{
    public async Task<Result> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var order = await db.Orders
                .Where(o => o.Id == command.OrderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (order is null)
            {
                return Result.Failure(OrderErrors.NotFound(command.OrderId));
            }

            var oldStatus = order.Status;

            if (!order.CanTransitionTo(command.NewStatus))
            {
                return Result.Failure(OrderErrors.InvalidStatusTransition(oldStatus, command.NewStatus));
            }

            order.Status = command.NewStatus;

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
        catch (Exception e)
        {
            logger.LogError(e, "Error changing order status for OrderId: {OrderId}", command.OrderId);

            return Result.Failure(OrderErrors.ChangeStatusFailed(command.OrderId,
                "An unexpected error occurred while changing the order status."));
        }
    }
}
