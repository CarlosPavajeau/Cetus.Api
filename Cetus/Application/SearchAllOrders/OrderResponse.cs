using System.Text;
using Cetus.Domain;

namespace Cetus.Application.SearchAllOrders;

public sealed record OrderResponse(Guid Id, OrderStatus Status, string Address, decimal Total, DateTime CreatedAt)
{
    public static OrderResponse FromOrder(Order order)
    {
        var address = new StringBuilder();
        address.Append(order.Address);

        if (order.City is null)
        {
            return new OrderResponse(order.Id, order.Status, address.ToString(), order.Total, order.CreatedAt);
        }

        address.Append(", ");
        address.Append(order.City.Name);
        address.Append(" - ");
        address.Append(order.City.State!.Name);

        return new OrderResponse(order.Id, order.Status, address.ToString(), order.Total, order.CreatedAt);
    }
}
