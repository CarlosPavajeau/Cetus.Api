using SharedKernel;

namespace Domain.Orders;

public sealed record OrderCreatedDomainEvent(Guid Id, long OrderNumber) : IDomainEvent;
