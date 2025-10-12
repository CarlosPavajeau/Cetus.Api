using Application.Abstractions.Messaging;

namespace Application.Orders.Pay;

public sealed record PayOrderCommand(Guid Id, string TransactionId) : ICommand<SimpleOrderResponse>;
