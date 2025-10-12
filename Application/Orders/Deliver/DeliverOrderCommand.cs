using Application.Abstractions.Messaging;

namespace Application.Orders.Deliver;

public sealed record DeliverOrderCommand(Guid Id) : ICommand<SimpleOrderResponse>;
