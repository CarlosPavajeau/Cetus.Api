using Application.Abstractions.Messaging;
using Application.Products.Create;

namespace Application.Products.CreateSimple;

public record CreateSimpleProductCommand(
    string Name,
    string? Description,
    Guid CategoryId,
    string Sku,
    decimal Price,
    int Stock,
    IReadOnlyList<CreateProductImage> Images
) : ICommand<ProductResponse>;
