using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllOrders;

public class SearchAllOrdersQueryHandler : IRequestHandler<SearchAllOrdersQuery, IEnumerable<OrderResponse>>
{
    private readonly CetusDbContext _context;

    public SearchAllOrdersQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderResponse>> Handle(SearchAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.City)
            .ThenInclude(c => c!.State)
            .OrderByDescending(order => order.Status)
            .ThenBy(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderResponse.FromOrder);
    }
}
