using MediatR;

namespace Cetus.Application.FindOrder;

public sealed record FindOrderQuery(Guid Id) : IRequest<OrderResponse?>;
