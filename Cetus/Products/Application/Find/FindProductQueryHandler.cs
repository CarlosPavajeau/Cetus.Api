using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Products.Application.SearchForSale;
using MediatR;

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
        var product = await _context.Products.FindAsync([request.Id],
            cancellationToken: cancellationToken);

        return product is null ? null : ProductResponse.FromProduct(product);
    }
}
