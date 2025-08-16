using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.ProductReviews.Create;

internal sealed class CreateProductReviewCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateProductReviewCommand>
{
    public async Task<Result> Handle(CreateProductReviewCommand command, CancellationToken cancellationToken)
    {
        var reviewRequest = await context.ReviewRequests
            .Include(rr => rr.OrderItem)
            .FirstOrDefaultAsync(rr => rr.Id == command.ReviewRequestId, cancellationToken);

        if (reviewRequest?.OrderItem is null)
        {
            return Result.Failure(ReviewRequestErrors.NotFound(command.ReviewRequestId.ToString()));
        }

        var productReview = new ProductReview
        {
            Id = Guid.NewGuid(),
            Comment = command.Comment,
            Rating = command.Rating,
            IsVerified = true,
            ReviewRequestId = command.ReviewRequestId,
            ProductId = reviewRequest.OrderItem.ProductId,
            CustomerId = reviewRequest.CustomerId,
            CreatedAt = DateTime.UtcNow
        };

        await context.ProductReviews.AddAsync(productReview, cancellationToken);

        reviewRequest.Status = ReviewRequestStatus.Completed;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
