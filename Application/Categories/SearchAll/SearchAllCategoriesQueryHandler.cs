using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.SearchAll;

internal sealed class SearchAllCategoriesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllCategoriesQuery, IEnumerable<CategoryResponse>>
{
    public async Task<Result<IEnumerable<CategoryResponse>>> Handle(SearchAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return Result.Success(categories.Select(CategoryResponse.FromCategory));
    }
}
