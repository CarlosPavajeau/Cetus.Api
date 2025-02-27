namespace Cetus.Application.SearchAllProducts;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool Enabled,
    DateTime CreatedAt,
    DateTime UpdatedAt);
