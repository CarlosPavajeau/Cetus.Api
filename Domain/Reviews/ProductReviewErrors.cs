using SharedKernel;

namespace Domain.Reviews;

public static class ProductReviewErrors
{
    public static Error NotFound(Guid Id) =>
        Error.NotFound(
            "ProductReviews.NotFound",
            $"Product review with ID {Id} was not found."
        );
}
