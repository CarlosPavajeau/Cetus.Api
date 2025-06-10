using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.Find;

internal sealed class FindReviewRequestQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindReviewRequestQuery, ReviewRequestResponse>
{
    public async Task<Result<ReviewRequestResponse>> Handle(FindReviewRequestQuery query,
        CancellationToken cancellationToken)
    {
        var reviewRequest = await context.ReviewRequests
            .Include(r => r.Customer)
            .Include(r => r.OrderItem)
            .FirstOrDefaultAsync(r => r.Token == query.Token, cancellationToken);

        if (reviewRequest is null)
        {
            return Result.Failure<ReviewRequestResponse>(ReviewRequestErrors.NotFound(query.Token));
        }

        return ReviewRequestResponse.FromReviewRequest(reviewRequest);
    }
}
