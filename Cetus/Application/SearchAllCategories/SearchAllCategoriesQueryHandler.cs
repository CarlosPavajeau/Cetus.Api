using Cetus.Domain;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllCategories;

public sealed class SearchAllCategoriesQueryHandler(CetusDbContext context)
    : IRequestHandler<SearchAllCategoriesQuery, IEnumerable<Category>>
{
    public async Task<IEnumerable<Category>> Handle(
        SearchAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Categories.ToListAsync(cancellationToken);
    }
}
