using Application.Abstractions.Messaging;
using Application.Products.SearchAll;

namespace Application.Products.Create;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    Guid CategoryId) : ICommand<ProductResponse>;
