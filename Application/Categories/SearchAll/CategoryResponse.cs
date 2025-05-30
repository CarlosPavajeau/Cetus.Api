using Domain.Categories;

namespace Application.Categories.SearchAll;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt)
{
    public static CategoryResponse FromCategory(Category category) =>
        new(category.Id, category.Name, category.CreatedAt, category.UpdatedAt, category.DeletedAt);
}
