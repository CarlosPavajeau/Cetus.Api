using Cetus.Domain;

namespace Cetus.Application.FindOrder;

public sealed record OrderCustomer(string Name, string Email, string Phone);

public sealed record OrderItem(Guid Id, string ProductName, string? ImageUrl, int Quantity, decimal Price);

public sealed record OrderResponse(
    Guid Id,
    OrderStatus Status,
    string Address,
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

        return new OrderResponse(order.Id, order.Status, order.Address, order.Total, orderItems, orderCustomer,
            order.TransactionId, order.CreatedAt);
    }
}
