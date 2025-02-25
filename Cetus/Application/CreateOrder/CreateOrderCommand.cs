using MediatR;

namespace Cetus.Application.CreateOrder;

public sealed record CreateOrderItem(string ProductName, int Quantity, decimal Price, Guid ProductId);

public sealed record CreateOrderCustomer(string Id, string Name, string Email, string Phone, string Address);

public sealed record CreateOrderCommand(
    string Address,
    decimal Total,
    IEnumerable<CreateOrderItem> Items,
    CreateOrderCustomer Customer)
    : IRequest<Guid>;
