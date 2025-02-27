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
            .Where(p => p.DeletedAt == null && p.Enabled && p.Stock > 0)
            .ToListAsync(cancellationToken);

        return products.Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.Stock));
    }
}
