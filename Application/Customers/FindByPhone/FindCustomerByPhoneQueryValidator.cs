using FluentValidation;

namespace Application.Customers.FindByPhone;

public sealed class FindCustomerByPhoneQueryValidator : AbstractValidator<FindCustomerByPhoneQuery>
{
    public FindCustomerByPhoneQueryValidator()
    {
        RuleFor(q => q.Phone)
            .NotEmpty()
            .WithMessage("Phone number must be provided.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number format is invalid.");
    }
}
