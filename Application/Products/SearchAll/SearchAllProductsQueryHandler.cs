using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchAll;

internal sealed class SearchAllProductsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(SearchAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.Products
            .AsNoTracking()
            .Where(p => p.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return Result.Success(products.Select(ProductResponse.FromProduct));
    }
}
