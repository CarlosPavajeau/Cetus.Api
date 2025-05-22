using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteCategoryCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories.FindAsync([request.Id], cancellationToken);
        if (category == null)
        {
            return false;
        }

        category.DeletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
