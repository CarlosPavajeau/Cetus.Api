using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchForSale;

internal sealed class SearchAllProductsForSaleQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<SearchAllProductsForSaleQuery, IEnumerable<SimpleProductForSaleResponse>>
{
    public async Task<Result<IEnumerable<SimpleProductForSaleResponse>>> Handle(SearchAllProductsForSaleQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.ProductVariants
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.DeletedAt == null
                        && p.Enabled
                        && p.Product!.Enabled
                        && p.Product!.DeletedAt == null
                        && p.Product!.StoreId == tenant.Id
            )
            .OrderByDescending(p => p.CreatedAt)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        var response = products
            .DistinctBy(p => p.Id)
            .ToList();

        return response;
    }
}
