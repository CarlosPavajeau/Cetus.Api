using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.SearchAll;

internal sealed class SearchAllProductVariantsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<SearchAllProductVariantsQuery, IEnumerable<ProductVariantResponse>>
{
    public async Task<Result<IEnumerable<ProductVariantResponse>>> Handle(SearchAllProductVariantsQuery query,
        CancellationToken cancellationToken)
    {
        bool productExists = await db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == query.ProductId && p.DeletedAt == null, cancellationToken);

        if (!productExists)
        {
            return Result.Failure<IEnumerable<ProductVariantResponse>>(
                ProductErrors.NotFound(query.ProductId.ToString()));
        }


        var variants = await db.ProductVariants
            .AsNoTracking()
            .Where(v => v.DeletedAt == null && v.ProductId == query.ProductId)
            .OrderBy(v => v.Price)
            .Select(ProductVariantResponse.Map)
            .ToListAsync(cancellationToken);

        return variants;
    }
}
