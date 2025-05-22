using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Domain.Orders;

namespace Application.Orders.Update;

public sealed record UpdateOrderCommand(Guid Id, OrderStatus Status, string? TransactionId = null)
    : ICommand<OrderResponse>;
