using System.Linq.Expressions;
using Domain.Orders;

namespace Application.Orders.SearchAll;

public sealed record OrderResponse(
    Guid Id,
    long OrderNumber,
    OrderStatus Status,
    string Address,
    string City,
    string State,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    DateTime CreatedAt)
{
    public static Expression<Func<Order, OrderResponse>> Map => order =>
        new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Address,
            order.City!.Name,
            order.City.State!.Name,
            order.Subtotal,
            order.Discount,
            order.Total,
            order.CreatedAt
        );
}
