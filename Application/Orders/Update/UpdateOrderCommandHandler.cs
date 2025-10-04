using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Update;

internal sealed class UpdateOrderCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateOrderCommand, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound(request.Id));
        }

        if (order.Status == OrderStatus.Canceled) // Can't update a canceled order
        {
            return Result.Failure<OrderResponse>(OrderErrors.InvalidStatusTransition(order.Status, request.Status));
        }

        order.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.TransactionId))
        {
            order.TransactionId = request.TransactionId;
        }

        var customer = await context.Customers.FindAsync([order.CustomerId], cancellationToken);
        if (customer is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.CustomerNotFound(order.CustomerId));
        }

        NotifyOrderStatusChanged(order, customer);

        await context.SaveChangesAsync(cancellationToken);

        return OrderResponse.FromOrder(order);
    }

    private static void NotifyOrderStatusChanged(Order order, Customer customer)
    {
        switch (order.Status)
        {
            case OrderStatus.Paid:
                order.Raise(new PaidOrderDomainEvent(new PaidOrder(
                    order.Id,
                    order.OrderNumber,
                    customer.Name,
                    customer.Email,
                    order.Total
                )));
                break;
            case OrderStatus.Delivered:
                order.Raise(new DeliveredOrderDomainEvent(new DeliveredOrder(
                    order.Id,
                    order.OrderNumber,
                    customer.Id,
                    customer.Email,
                    order.Items
                )));
                break;
            case OrderStatus.Pending:
            case OrderStatus.Canceled:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(order));
        }
    }
}
