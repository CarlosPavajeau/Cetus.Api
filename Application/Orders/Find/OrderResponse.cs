using System.Linq.Expressions;
using Domain.Orders;

namespace Application.Orders.Find;

public sealed record OrderCustomer(string Name, string Email, string Phone);

public sealed record OrderItem(Guid Id, string ProductName, string? ImageUrl, int Quantity, decimal Price);

public sealed record OrderResponse(
    Guid Id,
    long OrderNumber,
    OrderStatus Status,
    string Address,
    string City,
    string State,
    decimal Subtotal,
    decimal Discount,
    decimal DeliveryFee,
    decimal Total,
    IEnumerable<OrderItem> Items,
    OrderCustomer Customer,
    string? TransactionId,
    Guid StoreId,
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
            order.DeliveryFee,
            order.Total,
            order.Items.Select(item =>
                    new OrderItem(item.Id, item.ProductName, item.ImageUrl, item.Quantity, item.Price))
                .ToList(),
            new OrderCustomer(order.Customer!.Name, order.Customer.Email, order.Customer.Phone),
            order.TransactionId,
            order.StoreId,
            order.CreatedAt);
}
