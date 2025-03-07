using Cetus.Shared.Domain;

namespace Cetus.Domain.Events;

public sealed record PaidOrder(Guid Id, long OrderNumber, string Customer, decimal Total);

public sealed record PaidOrderEvent(PaidOrder Order, string CustomerEmail) : DomainEvent;
