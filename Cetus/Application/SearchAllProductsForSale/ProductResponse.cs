namespace Cetus.Application.SearchAllProductsForSale;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock);
