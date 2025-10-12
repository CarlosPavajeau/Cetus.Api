using Application.Abstractions.Messaging;

namespace Application.Orders.Cancel;

public sealed record CancelOrderCommand(Guid Id, string Reason) : ICommand<SimpleOrderResponse>;
