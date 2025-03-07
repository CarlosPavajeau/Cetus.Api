using Cetus.Shared.Domain;

namespace Cetus.Domain.Events;

public sealed record SentOrder(Guid Id, long OrderNumber, string Customer, string Address);

public sealed record SentOrderEvent(SentOrder Order, string CustomerEmail) : DomainEvent;
