using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateCategoryCommand, bool>
{
    public async Task<Result<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories.FindAsync([request.Id], cancellationToken);
        if (category == null)
        {
            return false;
        }

        category.Name = request.Name;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
