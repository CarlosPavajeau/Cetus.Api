using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.ProductReviews.Reject;

internal sealed class RejectProductReviewCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RejectProductReviewCommand>
{
    public async Task<Result> Handle(RejectProductReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await context.ProductReviews
            .FirstOrDefaultAsync(pr => pr.Id == command.Id, cancellationToken);

        if (review is null)
        {
            return Result.Failure(ProductReviewErrors.NotFound(command.Id));
        }

        var moderatorNotes = string.IsNullOrWhiteSpace(command.ModeratorNotes)
            ? null
            : command.ModeratorNotes.Trim();

        review.Reject(moderatorNotes);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
