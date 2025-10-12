using SharedKernel;

namespace Application.Orders.Cancel;

public record CanceledOrderDomainEvent(Guid OrderId) : IDomainEvent;
