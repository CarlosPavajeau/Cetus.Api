using Cetus.Domain;

namespace Cetus.Application.SearchAllOrders;

public sealed record OrderResponse(Guid Id, OrderStatus Status, string Address, decimal Total, DateTime CreatedAt);
