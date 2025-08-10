using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAllByCategory;

internal sealed class SearchAllProductsByCategoryHandler(IApplicationDbContext db)
    : IQueryHandler<SearchAllProductsByCategory, IEnumerable<SimpleProductForSaleResponse>>
{
    public async Task<Result<IEnumerable<SimpleProductForSaleResponse>>> Handle(SearchAllProductsByCategory query,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == query.CategoryId)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
