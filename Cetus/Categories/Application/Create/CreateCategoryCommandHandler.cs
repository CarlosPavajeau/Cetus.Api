using Cetus.Categories.Domain;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Categories.Application.Create;

internal sealed class CreateCategoryCommandHandler(CetusDbContext context) : IRequestHandler<CreateCategoryCommand, bool>
{
    public async Task<bool> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
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
