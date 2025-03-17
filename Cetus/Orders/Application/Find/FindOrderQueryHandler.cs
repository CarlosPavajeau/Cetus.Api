using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Orders.Application.Find;

internal sealed class FindOrderQueryHandler : IRequestHandler<FindOrderQuery, OrderResponse?>
{
    private readonly CetusDbContext _context;

    public FindOrderQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponse?> Handle(FindOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context
            .Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Include(o => o.City)
            .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return order is null ? null : OrderResponse.FromOrder(order);
    }
}
