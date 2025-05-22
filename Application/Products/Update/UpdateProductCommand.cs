using Application.Abstractions.Messaging;
using Application.Products.SearchAll;

namespace Application.Products.Update;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    Guid CategoryId,
    bool Enabled) : ICommand<ProductResponse?>;
