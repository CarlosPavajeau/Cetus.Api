using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Orders.Application.SearchAll;
using Cetus.Orders.Domain;
using Cetus.Orders.Domain.Events;
using MediatR;

namespace Cetus.Orders.Application.Update;

internal sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponse?>
{
    private readonly CetusDbContext _context;
    private readonly IMediator _mediator;

    public UpdateOrderCommandHandler(CetusDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<OrderResponse?> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync([request.Id], cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (order.Status == OrderStatus.Canceled) // Can't update a canceled order
        {
            return null;
        }

        order.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.TransactionId))
        {
            order.TransactionId = request.TransactionId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var customer = await _context.Customers.FindAsync([order.CustomerId], cancellationToken);
        if (customer is null)
        {
            return OrderResponse.FromOrder(order);
        }

        await NotifyOrderStatusChanged(order, customer, cancellationToken);

        return OrderResponse.FromOrder(order);
    }

    private async Task NotifyOrderStatusChanged(Order order, Customer customer, CancellationToken cancellationToken)
    {
        switch (order.Status)
        {
            case OrderStatus.Paid:
                await _mediator.Publish(
                    new PaidOrderEvent(
                        new PaidOrder(order.Id, order.OrderNumber, customer.Name, order.Total),
                        customer.Email
                    ), cancellationToken);
                break;
            case OrderStatus.Delivered:
                await _mediator.Publish(
                    new SentOrderEvent(
                        new SentOrder(order.Id, order.OrderNumber, customer.Name, order.Address),
                        customer.Email
                    ), cancellationToken);
                break;
            case OrderStatus.Pending:
            case OrderStatus.Canceled:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(order));
        }
    }
}
