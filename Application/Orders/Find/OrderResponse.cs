using System.Linq.Expressions;
using Application.Products.Variants;
using Domain.Orders;

namespace Application.Orders.Find;

public sealed record CustomerResponse(string Name, string Email, string Phone);

public sealed record OrderItemResponse(
    Guid Id,
    string ProductName,
    string? ImageUrl,
    int Quantity,
    decimal Price,
    long VariantId,
    IReadOnlyList<VariantOptionValueResponse> OptionValues);

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
    IReadOnlyList<OrderItemResponse> Items,
    CustomerResponse Customer,
    string? TransactionId,
    Guid StoreId,
    DateTime CreatedAt,
    string? CancellationReason,
    DateTime? CancelledAt)

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
                    new OrderItemResponse(
                        item.Id,
                        item.ProductName,
                        item.ImageUrl,
                        item.Quantity,
                        item.Price,
                        item.VariantId,
                        item.ProductVariant!.OptionValues
                            .Where(op => op.ProductOptionValue!.DeletedAt == null)
                            .Select(option =>
                                new VariantOptionValueResponse(
                                    option.OptionValueId,
                                    option.ProductOptionValue!.Value,
                                    option.ProductOptionValue!.OptionTypeId,
                                    option.ProductOptionValue!.ProductOptionType!.Name
                                )
                            ).ToList()
                    ))
                .ToList(),
            new CustomerResponse(
                order.Customer!.Name,
                order.Customer.Email,
                order.Customer.Phone
            ),
            order.TransactionId,
            order.StoreId,
            order.CreatedAt,
            order.CancellationReason,
            order.CancelledAt
        );
}
