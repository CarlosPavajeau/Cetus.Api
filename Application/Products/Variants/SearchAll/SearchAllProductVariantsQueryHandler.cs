using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.SearchAll;

internal sealed class SearchAllProductVariantsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<SearchAllProductVariantsQuery, IEnumerable<ProductVariantResponse>>
{
    public async Task<Result<IEnumerable<ProductVariantResponse>>> Handle(SearchAllProductVariantsQuery query,
        CancellationToken cancellationToken)
    {
        var variants = await db.ProductVariants
            .AsNoTracking()
            .Where(v => v.DeletedAt == null && v.ProductId == query.ProductId)
            .Select(ProductVariantResponse.Map)
            .ToListAsync(cancellationToken);

        return variants;
    }
}
