using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Products.Application.SearchForSale;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Products.Application.SearchSuggestions;

internal sealed class SearchProductSuggestionsQueryHandler : IRequestHandler<SearchProductSuggestionsQuery, IEnumerable<ProductResponse>>
{
    private readonly CetusDbContext _context;

    public SearchProductSuggestionsQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductResponse>> Handle(SearchProductSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.Id == request.ProductId)
            .Select(p => p.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (category == Guid.Empty)
        {
            return [];
        }
        
        // Select only 3 random products in the same category but not the same product
        var products = await _context.Products
            .Where(p => p.CategoryId == category && p.Id != request.ProductId)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .ToListAsync(cancellationToken);

        return products.Select(ProductResponse.FromProduct);
    }
}
