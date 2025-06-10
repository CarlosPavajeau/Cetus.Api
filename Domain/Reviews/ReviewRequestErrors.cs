using SharedKernel;

namespace Domain.Reviews;

public static class ReviewRequestErrors
{
    public static Error NotFound(string token) =>
        Error.NotFound(
            "Reviews.NotFound",
            $"Review request with token {token} was not found."
        );
}
