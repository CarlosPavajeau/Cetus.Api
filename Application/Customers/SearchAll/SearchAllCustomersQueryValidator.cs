using FluentValidation;

namespace Application.Customers.SearchAll;

public sealed class SearchAllCustomersQueryValidator : AbstractValidator<SearchAllCustomersQuery>
{
    public SearchAllCustomersQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThan(0)
            .WithMessage("La página debe ser mayor a cero.");

        RuleFor(q => q.PageSize)
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede ser mayor a 100.");

        RuleFor(q => q.SortBy)
            .IsInEnum()
            .When(q => q.SortBy is not null)
            .WithMessage("El campo de ordenamiento debe ser uno de: Name, TotalSpent, LastPurchase.");
    }
}
