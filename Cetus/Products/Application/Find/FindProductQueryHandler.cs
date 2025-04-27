using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Products.Application.Find;

internal sealed class FindProductQueryHandler : IRequestHandler<FindProductQuery, ProductResponse?>
{
    private readonly CetusDbContext _context;

    public FindProductQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponse?> Handle(FindProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return product is null ? null : ProductResponse.FromProduct(product);
    }
}
