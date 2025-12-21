using FluentValidation;

namespace Application.Products.Inventory.Transactions.SearchAll;

public sealed class SearchInventoryTransactionsQueryValidator : AbstractValidator<SearchInventoryTransactionsQuery>
{
    public SearchInventoryTransactionsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero");

        RuleFor(query => query.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than zero")
            .LessThan(100)
            .WithMessage("PageSize must be less than 100");
    }
}
