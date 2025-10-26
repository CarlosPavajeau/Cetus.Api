using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.Pay;

public sealed record PayOrderCommand(Guid Id, string TransactionId, PaymentProvider PaymentProvider)
    : ICommand<SimpleOrderResponse>;
