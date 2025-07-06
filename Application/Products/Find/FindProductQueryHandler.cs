using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Find;

internal sealed class FindProductQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindProductQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(FindProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Id == request.Id)
            .Select(ProductResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Id.ToString()));
        }

        return product;
    }
}
