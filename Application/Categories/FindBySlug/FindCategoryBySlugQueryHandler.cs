using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.FindBySlug;

internal sealed class FindCategoryBySlugQueryHandler(IApplicationDbContext db)
    : IQueryHandler<FindCategoryBySlugQuery, FindCategoryBySlugResponse>
{
    public async Task<Result<FindCategoryBySlugResponse>> Handle(FindCategoryBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .AsNoTracking()
            .Where(c => c.Slug == query.Slug)
            .Select(FindCategoryBySlugResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            return Result.Failure<FindCategoryBySlugResponse>(CategoryErrors.NotFound(query.Slug));
        }

        return category;
    }
}
