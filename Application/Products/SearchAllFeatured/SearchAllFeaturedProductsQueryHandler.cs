using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Find;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAllFeatured;

internal sealed class SearchAllFeaturedProductsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllFeaturedProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(SearchAllFeaturedProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.DeletedAt == null && p.Enabled && p.StoreId == tenant.Id)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.SalesCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(ProductResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
