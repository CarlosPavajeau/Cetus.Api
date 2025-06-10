using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.ProductReviews.Approve;

internal sealed class ApproveProductReviewCommandHandler(IApplicationDbContext context)
    : ICommandHandler<ApproveProductReviewCommand>
{
    public async Task<Result> Handle(ApproveProductReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await context.ProductReviews
            .FirstOrDefaultAsync(pr => pr.Id == command.Id, cancellationToken);

        if (review is null)
        {
            return Result.Failure(ProductReviewErrors.NotFound(command.Id));
        }

        review.Approve();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
