using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Products.Variants.Create;

internal sealed class CreateProductVariantCommandHandler(
    IApplicationDbContext db,
    ILogger<CreateProductVariantCommandHandler> logger
) : ICommandHandler<CreateProductVariantCommand>
{
    public async Task<Result> Handle(CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == command.ProductId, cancellationToken);
        if (!productExists)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId.ToString()));
        }

        var optionValuesExist = await db.ProductOptionValues
            .Where(v => command.OptionValueIds.Contains(v.Id))
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);

        if (optionValuesExist.Count != command.OptionValueIds.Length)
        {
            return Result.Failure(Error.Problem("Product.Variant", "Some option values do not exist"));
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        try
        {
            var variant = new ProductVariant
            {
                Sku = command.Sku,
                Price = command.Price,
                StockQuantity = command.StockQuantity,
                ProductId = command.ProductId
            };

            var variantOptionValues = command.OptionValueIds
                .Select(optionValueId => new ProductVariantOptionValue
                {
                    OptionValueId = optionValueId,
                    ProductVariant = variant
                })
                .ToList();

            var variantImages = command.Images
                .Select(image => new ProductImage
                {
                    ImageUrl = image.ImageUrl,
                    AltText = image.AltText,
                    SortOrder = image.SortOrder,
                    ProductVariant = variant
                })
                .ToList();

            await db.ProductVariants.AddAsync(variant, cancellationToken);
            await db.ProductVariantOptionsValues.AddRangeAsync(variantOptionValues, cancellationToken);
            await db.ProductImages.AddRangeAsync(variantImages, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured when creating a new product variant.");
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure(Error.None);
        }
    }
}
