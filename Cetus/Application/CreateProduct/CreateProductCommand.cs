using Cetus.Application.SearchAllProducts;
using MediatR;

namespace Cetus.Application.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    Guid CategoryId) : IRequest<ProductResponse>;
