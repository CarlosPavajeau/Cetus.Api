using MediatR;

namespace Cetus.Orders.Application.Find;

public sealed record FindOrderQuery(Guid Id) : IRequest<OrderResponse?>;
