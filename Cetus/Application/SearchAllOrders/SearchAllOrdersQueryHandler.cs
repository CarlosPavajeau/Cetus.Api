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
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(order =>
            new OrderResponse(order.Id, order.Status, order.Address, order.Total, order.CreatedAt));
    }
}
