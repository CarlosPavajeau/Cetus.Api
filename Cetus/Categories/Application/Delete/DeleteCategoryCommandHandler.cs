using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Categories.Application.Delete;

internal sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly CetusDbContext _context;

    public DeleteCategoryCommandHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync([request.Id], cancellationToken);
        if (category == null)
        {
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
