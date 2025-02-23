using Cetus.Domain;
using MediatR;

namespace Cetus.Application.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId) : IRequest<Product>;
