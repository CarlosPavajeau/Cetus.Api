using System.Text;
using Cetus.Orders.Domain;

namespace Cetus.Orders.Application.SearchAll;

public sealed record OrderResponse(
    Guid Id,
    long OrderNumber,
    OrderStatus Status,
    string Address,
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
            order.Total,
            order.CreatedAt);
    }
}
