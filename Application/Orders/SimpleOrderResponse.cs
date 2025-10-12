using Domain.Orders;

namespace Application.Orders;

public sealed record SimpleOrderResponse(
    Guid Id,
    long OrderNumber,
    OrderStatus Status,
    string Address,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    DateTime CreatedAt
)
{
    public static SimpleOrderResponse From(Order order) => new(
        order.Id,
        order.OrderNumber,
        order.Status,
        order.Address,
        order.Subtotal,
        order.Discount,
        order.Total,
        order.CreatedAt
    );
}
