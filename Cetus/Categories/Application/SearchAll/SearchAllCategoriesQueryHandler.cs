using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Categories.Application.SearchAll;

internal sealed class SearchAllCategoriesQueryHandler(CetusDbContext context)
    : IRequestHandler<SearchAllCategoriesQuery, IEnumerable<CategoryResponse>>
{
    public async Task<IEnumerable<CategoryResponse>> Handle(SearchAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return categories.Select(CategoryResponse.FromCategory);
    }
}
