using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllProducts;

public class SearchAllProductsQueryHandler(CetusDbContext context)
    : IRequestHandler<SearchAllProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<IEnumerable<ProductResponse>> Handle(SearchAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.Products
            .AsNoTracking()
            .Where(p => p.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return products.Select(ProductResponse.FromProduct);
    }
}
