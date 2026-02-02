using SharedKernel;

namespace Domain.Orders;

public sealed record SentOrder(Guid Id, long OrderNumber, string Customer, string? Address, string? CustomerEmail);

public sealed record SentOrderDomainEvent(SentOrder Order) : IDomainEvent;
