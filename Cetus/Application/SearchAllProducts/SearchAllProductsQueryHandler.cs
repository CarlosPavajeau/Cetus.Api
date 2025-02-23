using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllProducts;

public class SearchAllProductsQueryHandler(CetusDbContext context)
    : IRequestHandler<
        SearchAllProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<IEnumerable<ProductResponse>> Handle(
        SearchAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await context.Products
            .Where(p => p.Enabled && p.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return products.Select(p =>
            new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.Stock,
                p.CreatedAt, p.UpdatedAt));
    }
}
