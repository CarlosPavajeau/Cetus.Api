using FluentValidation;

namespace Application.Products.Search;

public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .WithMessage("Search term must be between 2 and 100 characters")
            .Matches(@"^[\p{L}\p{N}\s\-_,.]+$")
            .WithMessage("Search term contains invalid characters.");
    }
}
