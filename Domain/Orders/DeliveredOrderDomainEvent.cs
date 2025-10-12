using SharedKernel;

namespace Domain.Orders;

public sealed record DeliveredOrderDomainEvent(Guid Id) : IDomainEvent;
