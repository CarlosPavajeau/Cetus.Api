using Cetus.Application.SearchAllOrders;
using Cetus.Domain;
using Cetus.Domain.Events;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Application.UpdateOrder;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponse?>
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
        order.TransactionId = request.TransactionId;

        await _context.SaveChangesAsync(cancellationToken);

        if (order.Status != OrderStatus.Delivered)
        {
            return new OrderResponse(order.Id, order.OrderNumber, order.Status, order.Address, order.Total,
                order.CreatedAt);
        }

        var customer = await _context.Customers.FindAsync([order.CustomerId], cancellationToken);

        if (customer is not null)
        {
            await _mediator.Publish(
                new SentOrderEvent(
                    new SentOrder(order.Id, order.OrderNumber, customer.Name, order.Address),
                    customer.Email
                ),
                cancellationToken
            );
        }

        return new OrderResponse(order.Id, order.OrderNumber, order.Status, order.Address, order.Total,
            order.CreatedAt);
    }
}
