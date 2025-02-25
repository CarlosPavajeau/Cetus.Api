using Cetus.Application.SearchAllProducts;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Application.FindProduct;

public sealed class FindProductQueryHandler : IRequestHandler<FindProductQuery, ProductResponse?>
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

        return product is null
            ? null
            : new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.Stock,
                product.CreatedAt, product.UpdatedAt);
    }
}
