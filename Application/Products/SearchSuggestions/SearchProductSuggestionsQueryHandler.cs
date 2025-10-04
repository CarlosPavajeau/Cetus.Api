using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchSuggestions;

internal sealed class SearchProductSuggestionsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<SearchProductSuggestionsQuery, IEnumerable<SimpleProductForSaleResponse>>
{
    public async Task<Result<IEnumerable<SimpleProductForSaleResponse>>> Handle(SearchProductSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var category = await db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.Id == request.ProductId)
            .Select(p => p.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (category == Guid.Empty)
        {
            return Result.Success<IEnumerable<SimpleProductForSaleResponse>>([]);
        }

        // Select only 3 random products in the same category but not the same product
        var products = await db.ProductVariants
            .AsNoTracking()
            .Include(p => p.Product)
            .Where(p => p.DeletedAt == null
                        && p.Enabled
                        && p.Product!.Enabled
                        && p.Product!.DeletedAt == null
                        && p.Product!.CategoryId == category
                        && p.ProductId != request.ProductId
            )
            .OrderBy(_ => Guid.NewGuid())
            .Take(4)
            .Select(SimpleProductForSaleResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
