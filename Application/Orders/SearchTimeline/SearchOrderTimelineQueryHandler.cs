using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.SearchTimeline;

internal sealed class SearchOrderTimelineQueryHandler(IApplicationDbContext db)
    : IQueryHandler<SearchOrderTimelineQuery, IEnumerable<OrderTimelineResponse>>
{
    public async Task<Result<IEnumerable<OrderTimelineResponse>>> Handle(SearchOrderTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var timelines = await db.OrderTimeline
            .AsNoTracking()
            .Where(ot => ot.OrderId == query.OrderId)
            .Select(OrderTimelineResponse.Map)
            .ToListAsync(cancellationToken);

        return timelines;
    }
}
