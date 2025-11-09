using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.Images.Add;

internal sealed class AddVariantImagesCommandHandler(IApplicationDbContext db)
    : ICommandHandler<AddVariantImagesCommand>
{
    public async Task<Result> Handle(AddVariantImagesCommand command, CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants
            .AsNoTracking()
            .Where(p => p.Id == command.Id)
            .Select(v => new {v.Id, v.ProductId})
            .FirstOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return Result.Failure(ProductErrors.VariantNotFound(command.Id));
        }

        var newImages = command.Images.Select(image => new ProductImage
        {
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            SortOrder = image.SortOrder,
            VariantId = command.Id,
            ProductId = variant.ProductId
        });

        await db.ProductImages.AddRangeAsync(newImages, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
