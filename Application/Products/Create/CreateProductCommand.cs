using Application.Abstractions.Messaging;

namespace Application.Products.Create;

public sealed record CreateProductImage(
    string ImageUrl,
    string? AltText,
    int SortOrder
);

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    Guid CategoryId) : ICommand<ProductResponse>;
