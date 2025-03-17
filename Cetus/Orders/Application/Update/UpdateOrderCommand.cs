using Cetus.Orders.Application.SearchAll;
using Cetus.Orders.Domain;
using MediatR;

namespace Cetus.Orders.Application.Update;

public sealed record UpdateOrderCommand(Guid Id, OrderStatus Status, string? TransactionId = null)
    : IRequest<OrderResponse?>;
