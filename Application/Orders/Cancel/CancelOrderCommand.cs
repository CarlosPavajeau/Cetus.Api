using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;

namespace Application.Orders.Cancel;

public sealed record CancelOrderCommand(Guid Id) : ICommand<OrderResponse>;
