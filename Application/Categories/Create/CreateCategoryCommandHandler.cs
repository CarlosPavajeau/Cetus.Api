using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using SharedKernel;

namespace Application.Categories.Create;

internal sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateCategoryCommand, bool>
{
    public async Task<Result<bool>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        await context.Categories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
