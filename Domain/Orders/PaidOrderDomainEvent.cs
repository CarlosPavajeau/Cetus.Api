using SharedKernel;

namespace Domain.Orders;

public sealed record PaidOrder(Guid Id, long OrderNumber, string Customer, decimal Total);

public sealed record PaidOrderDomainEvent(PaidOrder Order) : IDomainEvent;
