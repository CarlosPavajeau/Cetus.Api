using System.Linq.Expressions;
using Domain.Categories;

namespace Application.Categories.SearchAll;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static Expression<Func<Category, CategoryResponse>> Map => category =>
        new CategoryResponse(category.Id, category.Name, category.CreatedAt, category.UpdatedAt);

    public static CategoryResponse FromCategory(Category category) => Map.Compile()(category);
}
