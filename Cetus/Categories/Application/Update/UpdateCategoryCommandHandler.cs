using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Categories.Application.Update;

internal sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly CetusDbContext _context;

    public UpdateCategoryCommandHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync([request.Id], cancellationToken);
        if (category == null)
        {
            return false;
        }

        category.Name = request.Name;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
