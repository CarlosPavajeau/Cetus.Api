using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllCategories;

public sealed class SearchAllCategoriesQueryHandler(CetusDbContext context)
    : IRequestHandler<SearchAllCategoriesQuery, IEnumerable<CategoryResponse>>
{
    public async Task<IEnumerable<CategoryResponse>> Handle(SearchAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryResponse(c.Id, c.Name, c.CreatedAt, c.UpdatedAt, c.DeletedAt));
    }
}
