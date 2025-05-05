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
        // Select only 5 random products in the same category but not the same product
        var products = await _context.Products
            .Where(p => p.CategoryId == request.CategoryId && p.Id != request.ProductId)
            .OrderBy(_ => Guid.NewGuid())
            .Take(5)
            .ToListAsync(cancellationToken);

        return products.Select(ProductResponse.FromProduct);
    }
}
