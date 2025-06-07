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
            .FirstOrDefaultAsync(x => x.Slug == request.Slug, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Slug));
        }

        return ProductResponse.FromProduct(product);
    }
} 
