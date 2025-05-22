using Application.Abstractions.Messaging;

namespace Application.Orders.SearchAll;

public sealed record SearchAllOrdersQuery : IQuery<IEnumerable<OrderResponse>>;
