using Cetus.Orders.Application.SearchAll;
using MediatR;

namespace Cetus.Orders.Application.Create;

public sealed record CreateOrderItem(string ProductName, string? ImageUrl, int Quantity, decimal Price, Guid ProductId);

public sealed record CreateOrderCustomer(string Id, string Name, string Email, string Phone, string Address);

public sealed record CreateOrderCommand(
    string Address,
    Guid CityId,
    decimal Total,
    IEnumerable<CreateOrderItem> Items,
    CreateOrderCustomer Customer)
    : IRequest<OrderResponse>;
