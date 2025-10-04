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
        var products = await db.ProductVariants
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.DeletedAt == null
                        && p.Enabled
                        && p.Product!.Enabled
                        && p.Product!.DeletedAt == null
                        && p.Product!.StoreId == tenant.Id
            )
            .OrderByDescending(p => p.SalesCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(4)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
