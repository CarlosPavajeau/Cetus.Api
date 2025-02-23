namespace Cetus.Application.SearchAllCategories;

public sealed record class CategoryResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt);
