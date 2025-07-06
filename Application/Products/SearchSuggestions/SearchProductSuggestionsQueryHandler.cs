using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Find;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.SearchSuggestions;

internal sealed class SearchProductSuggestionsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchProductSuggestionsQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(SearchProductSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var category = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.Id == request.ProductId)
            .Select(p => p.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (category == Guid.Empty)
        {
            return Result.Success<IEnumerable<ProductResponse>>([]);
        }

        // Select only 3 random products in the same category but not the same product
        var products = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.DeletedAt == null && p.Enabled && p.Stock > 0)
            .Where(p => p.CategoryId == category && p.Id != request.ProductId)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .Select(ProductResponse.Map)
            .ToListAsync(cancellationToken);

        return products;
    }
}
