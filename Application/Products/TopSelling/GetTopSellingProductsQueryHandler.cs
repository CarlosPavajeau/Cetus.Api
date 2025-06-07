using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.SearchForSale;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.TopSelling;

internal sealed class GetTopSellingProductsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTopSellingProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(GetTopSellingProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.DeletedAt == null && p.Enabled && p.SalesCount > 0)
            .OrderByDescending(p => p.SalesCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Result.Success(products.Select(ProductResponse.FromProduct));
    }
} 
