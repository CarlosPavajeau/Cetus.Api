using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.ChangeStatus;

public sealed record ChangeOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus,
    PaymentMethod? PaymentMethod,
    string? UserId,
    string? Notes
) : ICommand;
