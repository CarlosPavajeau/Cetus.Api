using Cetus.Products.Application.SearchAll;
using MediatR;

namespace Cetus.Products.Application.Update;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    Guid CategoryId,
    bool Enabled) : IRequest<ProductResponse?>;
