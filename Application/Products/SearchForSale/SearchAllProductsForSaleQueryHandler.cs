using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchForSale;

internal sealed class SearchAllProductsForSaleQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllProductsForSaleQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(SearchAllProductsForSaleQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.DeletedAt == null && p.Enabled && p.Stock > 0)
            .ToListAsync(cancellationToken);

        return Result.Success(products.Select(ProductResponse.FromProduct));
    }
}
