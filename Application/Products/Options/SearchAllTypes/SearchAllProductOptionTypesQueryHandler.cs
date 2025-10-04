using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Options.SearchAllTypes;

internal sealed class SearchAllProductOptionTypesQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchAllProductOptionTypesQuery, IEnumerable<ProductOptionTypeResponse>>
{
    public async Task<Result<IEnumerable<ProductOptionTypeResponse>>> Handle(SearchAllProductOptionTypesQuery query,
        CancellationToken cancellationToken)
    {
        var types = await db.ProductOptionTypes
            .AsNoTracking()
            .Where(p => p.StoreId == tenant.Id)
            .OrderBy(p => p.CreatedAt)
            .Select(ProductOptionTypeResponse.Map)
            .ToListAsync(cancellationToken);

        return types;
    }
}
