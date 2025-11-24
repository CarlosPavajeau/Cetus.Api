using Domain.Orders;
using FluentValidation;

namespace Application.Orders.SearchAll;

public sealed class SearchAllOrdersQueryValidator : AbstractValidator<SearchAllOrdersQuery>
{
    public SearchAllOrdersQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero");

        RuleFor(query => query.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than zero")
            .LessThan(100)
            .WithMessage("PageSize must be less than 100");

        RuleForEach(query => query.Statuses)
            .Must(status => Enum.TryParse<OrderStatus>(status, ignoreCase: true, out _))
            .WithMessage(status => $"Invalid order status: {status}");
    }
}
