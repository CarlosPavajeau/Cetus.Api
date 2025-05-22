using FluentValidation;

namespace Application.Categories.Update;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .NotEmpty()
            .WithMessage("El nombre de la categoría es requerido.");
    }
}
