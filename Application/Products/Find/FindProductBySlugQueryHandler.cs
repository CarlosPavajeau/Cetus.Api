using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Find;

internal sealed class FindProductBySlugQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindProductBySlugQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(FindProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(p => p.Slug == request.Slug)
            .Select(ProductResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Slug));
        }

        return product;
    }
} 
