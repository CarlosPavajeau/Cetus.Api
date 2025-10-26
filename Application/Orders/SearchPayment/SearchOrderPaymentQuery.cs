using Application.Abstractions.Messaging;

namespace Application.Orders.SearchPayment;

public sealed record SearchOrderPaymentQuery(Guid Id) : IQuery<OrderPaymentResponse>;
