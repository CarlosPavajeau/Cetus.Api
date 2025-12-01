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
            .Matches(@"^[a-zA-Z0-9\s\-_,.]+$")
            .WithMessage("Search term contains invalid characters.");
    }
}
