using System.Linq.Expressions;
using Domain.Categories;

namespace Application.Categories.FindBySlug;

public sealed record FindCategoryBySlugResponse(Guid Id, string Name, string Slug, Guid StoreId)
{
    public static Expression<Func<Category, FindCategoryBySlugResponse>> Map => category =>
        new FindCategoryBySlugResponse(category.Id, category.Name, category.Slug, category.StoreId);
}
