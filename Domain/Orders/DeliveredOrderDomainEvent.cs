using SharedKernel;

namespace Domain.Orders;

public sealed record DeliveredOrder(
    Guid Id,
    long OrderNumber,
    string CustomerId,
    string CustomerEmail,
    IEnumerable<OrderItem> Items);

public sealed record DeliveredOrderDomainEvent(DeliveredOrder Order) : IDomainEvent;
