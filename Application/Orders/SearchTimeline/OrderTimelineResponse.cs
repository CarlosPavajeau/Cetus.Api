using System.Linq.Expressions;
using Domain.Orders;

namespace Application.Orders.SearchTimeline;

public sealed record OrderTimelineResponse(
    Guid Id,
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    string? Notes,
    string? ChangedById,
    DateTime CreatedAt
)
{
    public static Expression<Func<OrderTimeline, OrderTimelineResponse>> Map => from =>
        new OrderTimelineResponse(
            from.Id,
            from.FromStatus,
            from.ToStatus,
            from.Notes,
            from.ChangedById,
            from.CreatedAt
        );
}
