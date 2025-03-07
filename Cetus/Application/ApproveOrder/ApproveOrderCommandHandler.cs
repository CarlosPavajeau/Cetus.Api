using Cetus.Domain;
using Cetus.Domain.Events;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.ApproveOrder;

public sealed class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, bool>
{
    private readonly CetusDbContext _context;
    private readonly IMediator _mediator;

    public ApproveOrderCommandHandler(CetusDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.Status = OrderStatus.Paid;
        order.TransactionId = request.TransactionId;

        await _context.SaveChangesAsync(cancellationToken);

        if (order.Customer is not null)
        {
            await _mediator.Publish(
                new PaidOrderEvent(
                    new PaidOrder(order.Id, order.OrderNumber, order.Customer.Name, order.Total),
                    order.Customer.Email
                ),
                cancellationToken
            );
        }

        return true;
    }
}
