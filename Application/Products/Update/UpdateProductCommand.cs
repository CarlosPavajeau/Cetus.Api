using Application.Abstractions.Messaging;
using Application.Products.SearchAll;

namespace Application.Products.Update;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    bool Enabled) : ICommand<ProductResponse>;
