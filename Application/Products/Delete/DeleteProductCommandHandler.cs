using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Delete;

internal sealed class DeleteProductCommandHandler(IApplicationDbContext context) : ICommandHandler<DeleteProductCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([request.Id], cancellationToken);
        if (product == null)
        {
            return Result.Failure<bool>(ProductErrors.NotFound(request.Id.ToString()));
        }

        product.DeletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
