using Cetus.Application.SearchAllOrders;
using Cetus.Domain;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Application.UpdateOrder;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponse?>
{
    private readonly CetusDbContext _context;

    public UpdateOrderCommandHandler(CetusDbContext context)
    {
        _context = context;
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

        await _context.SaveChangesAsync(cancellationToken);

        return new OrderResponse(order.Id, order.Status, order.Address, order.Total, order.CreatedAt);
    }
}
