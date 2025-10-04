using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAllFeatured;

internal sealed class SearchAllFeaturedProductsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllFeaturedProductsQuery, IEnumerable<SimpleProductForSaleResponse>>
{
    public async Task<Result<IEnumerable<SimpleProductForSaleResponse>>> Handle(SearchAllFeaturedProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await db.ProductVariants
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.DeletedAt == null
                        && p.Enabled
                        && p.Featured
                        && p.Product!.Enabled
                        && p.Product!.DeletedAt == null
                        && p.Product!.StoreId == tenant.Id
            )
            .Take(10)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
