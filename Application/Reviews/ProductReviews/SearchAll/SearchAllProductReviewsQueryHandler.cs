using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.ProductReviews.SearchAll;

internal sealed class SearchAllProductReviewsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllProductReviewsQuery, IEnumerable<ProductReviewResponse>>
{
    public async Task<Result<IEnumerable<ProductReviewResponse>>> Handle(SearchAllProductReviewsQuery query,
        CancellationToken cancellationToken)
    {
        var productReviews = await context.ProductReviews
            .Include(pr => pr.Customer)
            .Where(pr => pr.ProductId == query.ProductId && pr.Status == ProductReviewStatus.Approved)
            .OrderBy(pr => pr.CreatedAt)
            .Select(ProductReviewResponse.Map)
            .ToListAsync(cancellationToken);
        
        return productReviews;
    }
}
