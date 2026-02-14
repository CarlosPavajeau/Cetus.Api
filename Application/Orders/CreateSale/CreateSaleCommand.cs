using Application.Abstractions.Messaging;
using Domain.Customers;
using Domain.Orders;

namespace Application.Orders.CreateSale;

public sealed record CreateSaleItem(long VariantId, int Quantity);

public sealed record CreateSaleCustomer(
    string Phone,
    string Name,
    string? Email = null,
    DocumentType? DocumentType = null,
    string? DocumentNumber = null
);

public sealed record CreateSaleShipping(
    string Address,
    Guid? CityId = null
);

public sealed record CreateSaleCommand(
    IReadOnlyList<CreateSaleItem> Items,
    CreateSaleCustomer Customer,
    OrderChannel Channel,
    PaymentMethod PaymentMethod,
    CreateSaleShipping? Shipping = null,
    PaymentStatus PaymentStatus = PaymentStatus.Pending
) : ICommand<SimpleOrderResponse>;
