using Application.Abstractions.Messaging;
using Domain.Customers;

namespace Application.Orders.Create;

public sealed record CreateOrderItem(long VariantId, int Quantity);

public sealed record CreateOrderCustomer(
    string Phone,
    string Name,
    string? Email = null,
    DocumentType? DocumentType = null,
    string? DocumentNumber = null
);

public sealed record CreateOrderShipping(
    string Address,
    Guid CityId
);

public sealed record CreateOrderCommand(
    IReadOnlyList<CreateOrderItem> Items,
    CreateOrderCustomer Customer,
    CreateOrderShipping Shipping
) : ICommand<SimpleOrderResponse>;
