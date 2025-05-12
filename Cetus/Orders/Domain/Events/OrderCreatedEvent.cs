using Cetus.Shared.Domain;

namespace Cetus.Orders.Domain.Events;

public sealed record OrderCreated(Guid Id, long OrderNumber);

public sealed record OrderCreatedEvent(OrderCreated Order) : DomainEvent;
