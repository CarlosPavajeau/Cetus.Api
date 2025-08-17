using Application.Abstractions.Messaging;
using Application.Products.Create;
using Application.Products.SearchAll;

namespace Application.Products.Update;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    List<CreateProductImage> Images,
    Guid CategoryId,
    bool Enabled) : ICommand<ProductResponse>;
