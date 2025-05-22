using Application.Abstractions.Messaging;

namespace Application.Orders.Find;

public sealed record FindOrderQuery(Guid Id) : IQuery<OrderResponse>;
