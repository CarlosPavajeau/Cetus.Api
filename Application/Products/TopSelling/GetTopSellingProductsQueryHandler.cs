using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.TopSelling;

internal sealed class GetTopSellingProductsQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<GetTopSellingProductsQuery, IEnumerable<TopSellingProductResponse>>
{
    public async Task<Result<IEnumerable<TopSellingProductResponse>>> Handle(GetTopSellingProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.ProductVariants
            .AsNoTracking()
            .Where(p => p.DeletedAt == null
                        && p.Product!.DeletedAt == null
                        && p.SalesCount > 0
                        && p.Product!.StoreId == tenant.Id
            )
            .OrderByDescending(v => v.SalesCount)
            .Take(5)
            .Select(TopSellingProductResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
