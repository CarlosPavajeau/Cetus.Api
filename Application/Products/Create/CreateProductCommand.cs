using Application.Abstractions.Messaging;
using Application.Products.SearchAll;

namespace Application.Products.Create;

public sealed record CreateProductImage(
    string ImageUrl,
    string? AltText,
    int SortOrder
);

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    List<CreateProductImage> Images,
    Guid CategoryId) : ICommand<ProductResponse>;
