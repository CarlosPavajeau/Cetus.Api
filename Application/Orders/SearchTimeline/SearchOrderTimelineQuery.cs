using Application.Abstractions.Messaging;

namespace Application.Orders.SearchTimeline;

public sealed record SearchOrderTimelineQuery(Guid OrderId) : IQuery<IEnumerable<OrderTimelineResponse>>;
