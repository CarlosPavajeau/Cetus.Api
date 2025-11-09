using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.Images.Delete;

internal sealed class DeleteVariantImagesCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteVariantImageCommand>
{
    public async Task<Result> Handle(DeleteVariantImageCommand command, CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants
            .AsNoTracking()
            .Where(p => p.Id == command.VariantId)
            .Select(v => new {v.Id, v.ProductId})
            .FirstOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return Result.Failure(ProductErrors.VariantNotFound(command.VariantId));
        }

        await db.ProductImages
            .Where(i => i.ProductId == variant.ProductId &&
                        i.VariantId == command.VariantId &&
                        i.Id == command.ImageId
            )
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}
