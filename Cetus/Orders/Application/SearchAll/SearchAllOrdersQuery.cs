using MediatR;

namespace Cetus.Orders.Application.SearchAll;

public sealed record SearchAllOrdersQuery : IRequest<IEnumerable<OrderResponse>>;
