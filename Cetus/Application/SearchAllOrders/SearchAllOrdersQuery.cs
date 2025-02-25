using MediatR;

namespace Cetus.Application.SearchAllOrders;

public sealed record SearchAllOrdersQuery : IRequest<IEnumerable<OrderResponse>>;
