using Cetus.Application.SearchAllOrders;
using Cetus.Domain;
using MediatR;

namespace Cetus.Application.UpdateOrder;

public sealed record UpdateOrderCommand(Guid Id, OrderStatus Status, string? TransactionId = null)
    : IRequest<OrderResponse?>;
