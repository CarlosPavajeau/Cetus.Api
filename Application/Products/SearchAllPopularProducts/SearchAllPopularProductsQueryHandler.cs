using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Find;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAllPopularProducts;

internal sealed class SearchAllPopularProductsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
: IQueryHandler<SearchAllPopularProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(SearchAllPopularProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.DeletedAt == null && p.Enabled && p.StoreId == tenant.Id)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.SalesCount)
            .Take(4)
            .Select(ProductResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
