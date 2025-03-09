using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllProductsForSale;

public class
    SearchAllProductsForSaleQueryHandler : IRequestHandler<SearchAllProductsForSaleQuery, IEnumerable<ProductResponse>>
{
    private readonly CetusDbContext _context;

    public SearchAllProductsForSaleQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductResponse>> Handle(SearchAllProductsForSaleQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.DeletedAt == null && p.Enabled && p.Stock > 0)
            .ToListAsync(cancellationToken);

        return products.Select(ProductResponse.FromProduct);
    }
}
