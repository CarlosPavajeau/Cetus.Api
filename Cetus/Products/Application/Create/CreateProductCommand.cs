using Cetus.Products.Application.SearchAll;
using MediatR;

namespace Cetus.Products.Application.Create;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    Guid CategoryId) : IRequest<ProductResponse>;
