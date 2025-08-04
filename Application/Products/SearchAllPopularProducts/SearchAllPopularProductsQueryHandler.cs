using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAllPopularProducts;

internal sealed class SearchAllPopularProductsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllPopularProductsQuery, IEnumerable<SimpleProductForSaleResponse>>
{
    public async Task<Result<IEnumerable<SimpleProductForSaleResponse>>> Handle(SearchAllPopularProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Include(p => p.Images.OrderBy(i => i.SortOrder).Take(1))
            .Where(p => p.DeletedAt == null && p.Enabled && p.StoreId == tenant.Id)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.SalesCount)
            .Take(4)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
