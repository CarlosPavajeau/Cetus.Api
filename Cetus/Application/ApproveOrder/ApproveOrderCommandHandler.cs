using Cetus.Domain;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Application.ApproveOrder;

public sealed class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, bool>
{
    private readonly CetusDbContext _context;

    public ApproveOrderCommandHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync([request.Id], cancellationToken);
        if (order is null)
        {
            return false;
        }

        order.Status = OrderStatus.Paid;
        order.TransactionId = request.TransactionId;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
