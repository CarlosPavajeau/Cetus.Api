using System.Text;
using Domain.Orders;

namespace Application.Orders.SearchAll;

public sealed record OrderResponse(
    Guid Id,
    long OrderNumber,
    OrderStatus Status,
    string Address,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    DateTime CreatedAt)
{
    public static OrderResponse FromOrder(Order order)
    {
        var address = new StringBuilder();
        address.Append(order.Address);

        if (order.City is null)
        {
            return new OrderResponse(
                order.Id,
                order.OrderNumber,
                order.Status,
                address.ToString(),
                order.Subtotal,
                order.Discount,
                order.Total,
                order.CreatedAt);
        }

        address.Append(", ");
        address.Append(order.City.Name);
        address.Append(" - ");
        address.Append(order.City.State!.Name);

        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            address.ToString(),
            order.Subtotal,
            order.Discount,
            order.Total,
            order.CreatedAt);
    }
}
