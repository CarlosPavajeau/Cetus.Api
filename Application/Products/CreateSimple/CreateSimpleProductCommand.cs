using Application.Abstractions.Messaging;
using Application.Products.Create;
using Application.Products.SearchAll;

namespace Application.Products.CreateSimple;

public record CreateSimpleProductCommand(
    string Name,
    string? Description,
    Guid CategoryId,
    string Sku,
    decimal Price,
    int StockQuantity,
    IReadOnlyList<CreateProductImage> Images
) : ICommand<ProductResponse>;
