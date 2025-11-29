using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Products.Variants.Images.Order;

internal sealed class OrderVariantImagesCommandHandler(
    IApplicationDbContext db,
    ILogger<OrderVariantImagesCommandHandler> logger)
    : ICommandHandler<OrderVariantImagesCommand>
{
    public async Task<Result> Handle(OrderVariantImagesCommand command, CancellationToken cancellationToken)
    {
        var newImages = command.Images.ToDictionary(i => i.Id, i => i.SortOrder);
        var newImageIds = newImages.Keys;

        var images = await db.ProductImages
            .Where(i => i.VariantId == command.VariantId && newImageIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var image in images)
        {
            if (!newImages.TryGetValue(image.Id, out int newOrder))
            {
                logger.LogWarning(
                    "Image with ID {ImageId} not found in the new images list for variant {VariantId}. Skipping.",
                    image.Id, command.VariantId);
                continue;
            }

            image.SortOrder = newOrder;
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
