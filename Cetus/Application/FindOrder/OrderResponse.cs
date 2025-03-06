using System.Text;
using Cetus.Domain;

namespace Cetus.Application.FindOrder;

public sealed record OrderCustomer(string Name, string Email, string Phone);

public sealed record OrderItem(Guid Id, string ProductName, string? ImageUrl, int Quantity, decimal Price);

public sealed record OrderResponse(
    Guid Id,
    long OrderNumber,
    OrderStatus Status,
    string Address,
    decimal DeliveryFee,
    decimal Total,
    IEnumerable<OrderItem> Items,
    OrderCustomer Customer,
    string? TransactionId,
    DateTime CreatedAt)

{
    public static OrderResponse FromOrder(Order order)
    {
        var orderCustomer = order.Customer is null
            ? new OrderCustomer(string.Empty, string.Empty, string.Empty)
            : new OrderCustomer(order.Customer.Name, order.Customer.Email, order.Customer.Phone);

        var orderItems =
            order.Items.Select(item =>
                new OrderItem(item.Id, item.ProductName, item.ImageUrl, item.Quantity, item.Price));

        var address = new StringBuilder();
        address.Append(order.Address);

        if (order.City is null)
        {
            return new OrderResponse(
                order.Id,
                order.OrderNumber,
                order.Status,
                address.ToString(),
                order.DeliveryFee,
                order.Total,
                orderItems,
                orderCustomer,
                order.TransactionId,
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
            order.DeliveryFee,
            order.Total,
            orderItems,
            orderCustomer,
            order.TransactionId,
            order.CreatedAt);
    }
}
