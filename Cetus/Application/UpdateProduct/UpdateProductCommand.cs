using Cetus.Application.SearchAllProducts;
using MediatR;

namespace Cetus.Application.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool Enable) : IRequest<ProductResponse?>;
