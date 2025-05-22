using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Find;

internal sealed class FindOrderQueryHandler : IQueryHandler<FindOrderQuery, OrderResponse>
{
    private readonly IApplicationDbContext _context;

    public FindOrderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderResponse>> Handle(FindOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context
            .Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Include(o => o.City)
            .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound(request.Id));
        }

        return OrderResponse.FromOrder(order);
    }
}
