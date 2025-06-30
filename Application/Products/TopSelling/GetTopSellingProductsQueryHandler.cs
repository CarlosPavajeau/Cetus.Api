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
        var products = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.DeletedAt == null && p.Enabled && p.SalesCount > 0 && p.StoreId == tenant.Id)
            .OrderByDescending(p => p.SalesCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Result.Success(products.Select(TopSellingProductResponse.FromProduct));
    }
}
