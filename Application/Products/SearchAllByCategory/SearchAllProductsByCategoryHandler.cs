using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAllByCategory;

internal sealed class SearchAllProductsByCategoryHandler(IApplicationDbContext db)
    : IQueryHandler<SearchAllProductsByCategory, IEnumerable<SimpleProductForSaleResponse>>
{
    public async Task<Result<IEnumerable<SimpleProductForSaleResponse>>> Handle(SearchAllProductsByCategory query,
        CancellationToken cancellationToken)
    {
        var products = await db.ProductVariants
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.Product!.CategoryId == query.CategoryId)
            .Select(SimpleProductForSaleResponse.MapV)
            .ToListAsync(cancellationToken);

        var response = products
            .DistinctBy(p => p.Id)
            .ToList();

        return response;
    }
}
