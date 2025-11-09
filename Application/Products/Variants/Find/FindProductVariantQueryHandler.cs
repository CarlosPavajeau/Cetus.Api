using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.Find;

internal sealed class FindProductVariantQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<FindProductVariantQuery, ProductVariantResponse>
{
    public async Task<Result<ProductVariantResponse>> Handle(FindProductVariantQuery query,
        CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants
            .Where(v => v.Id == query.Id)
            .Select(ProductVariantResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return Result.Failure<ProductVariantResponse>(ProductErrors.VariantNotFound(query.Id));
        }

        return variant;
    }
}
