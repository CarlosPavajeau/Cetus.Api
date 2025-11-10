using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.Images.Add;

internal sealed class AddVariantImagesCommandHandler(IApplicationDbContext db)
    : ICommandHandler<AddVariantImagesCommand, AddVariantImagesCommandResponse>
{
    public async Task<Result<AddVariantImagesCommandResponse>> Handle(AddVariantImagesCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants
            .AsNoTracking()
            .Where(p => p.Id == command.Id)
            .Select(v => new {v.Id, v.ProductId})
            .FirstOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return Result.Failure<AddVariantImagesCommandResponse>(ProductErrors.VariantNotFound(command.Id));
        }

        var newImages = command.Images.Select(image => new ProductImage
        {
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            SortOrder = image.SortOrder,
            VariantId = command.Id,
            ProductId = variant.ProductId
        }).ToList();

        await db.ProductImages.AddRangeAsync(newImages, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        var responseImages = newImages
            .Select(img => new ProductImageResponse(img.Id, img.ImageUrl, img.AltText, img.SortOrder))
            .ToList();
        
        var response = new AddVariantImagesCommandResponse(variant.Id, responseImages);

        return response;
    }
}
