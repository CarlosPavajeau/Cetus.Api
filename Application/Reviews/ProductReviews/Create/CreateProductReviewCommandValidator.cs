using FluentValidation;

namespace Application.Reviews.ProductReviews.Create;

public sealed class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
{
    public CreateProductReviewCommandValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Rating)
            .InclusiveBetween((byte) 1, (byte) 5)
            .WithMessage("La calificación debe estar entre 1 y 5.");

        RuleFor(x => x.ReviewRequestId).NotEmpty();
    }
}
