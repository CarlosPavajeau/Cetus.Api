using Application.Abstractions.Messaging;

namespace Application.Orders.Create;

public sealed record CreateOrderItem(string ProductName, string? ImageUrl, int Quantity, decimal Price, long VariantId);

public sealed record CreateOrderCustomer(string Id, string Name, string Email, string Phone, string Address);

public sealed record CreateOrderCommand(
    string Address,
    Guid CityId,
    decimal Total,
    IReadOnlyList<CreateOrderItem> Items,
    CreateOrderCustomer Customer)
    : ICommand<SimpleOrderResponse>;
