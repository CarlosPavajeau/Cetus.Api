using Application.Abstractions.Messaging;

namespace Application.Orders.CreatePayment;

public sealed record CreateOrderPaymentCommand(Guid Id) : ICommand<string>;
