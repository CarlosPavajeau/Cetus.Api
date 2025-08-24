using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Products.Variants.Create;

internal sealed class CreateProductVariantCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    ILogger<CreateProductVariantCommandHandler> logger
) : ICommandHandler<CreateProductVariantCommand>
{
    public async Task<Result> Handle(CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var productExists = await db.Products
            .AnyAsync(p => p.Id == command.ProductId && p.StoreId == tenant.Id, cancellationToken);

        if (!productExists)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId.ToString()));
        }

        var normalizedSku = command.Sku.Trim().ToLowerInvariant();
        var skuExists = await db.ProductVariants
            .AsNoTracking()
            .AnyAsync(v =>
                    v.ProductId == command.ProductId &&
                    v.Sku == normalizedSku &&
                    v.DeletedAt == null,
                cancellationToken);

        if (skuExists)
        {
            return Result.Failure(ProductVariantErrors.DuplicateSku(normalizedSku));
        }

        // De-duplicate to avoid false negatives and duplicate join rows
        var distinctOptionValueIds = command.OptionValueIds.Distinct().ToArray();

        // Load option values with their OptionType and Store context
        var optionValueInfos = await db.ProductOptionValues
            .AsNoTracking()
            .Where(v => distinctOptionValueIds.Contains(v.Id))
            .Select(v => new {v.Id, v.OptionTypeId, v.ProductOptionType!.StoreId})
            .ToListAsync(cancellationToken);

        // All must exist
        if (optionValueInfos.Count != distinctOptionValueIds.Length)
        {
            return Result.Failure(ProductVariantErrors.MissingOptionValues());
        }

        // All must belong to current tenant/store
        if (optionValueInfos.Any(v => v.StoreId != tenant.Id))
        {
            return Result.Failure(ProductVariantErrors.OptionValuesCrossStore());
        }

        // All option types must be attached to the product
        var attachedOptionTypeIds = await db.ProductOptions
            .AsNoTracking()
            .Where(po => po.ProductId == command.ProductId)
            .Select(po => po.OptionTypeId)
            .ToListAsync(cancellationToken);

        if (optionValueInfos.Any(v => !attachedOptionTypeIds.Contains(v.OptionTypeId)))
        {
            return Result.Failure(ProductVariantErrors.OptionTypesNotAttached());
        }

        if (distinctOptionValueIds.Length > 0)
        {
            var candidates = await db.ProductVariantOptionValues
                .AsNoTracking()
                .Where(x => x.ProductVariant!.ProductId == command.ProductId)
                .GroupBy(x => x.VariantId)
                .Select(g => new
                {
                    VariantId = g.Key,
                    Total = g.Count(),
                    Matched = g.Count(x => distinctOptionValueIds.Contains(x.OptionValueId))
                })
                .ToListAsync(cancellationToken);

            if (candidates.Any(c =>
                    c.Total == distinctOptionValueIds.Length && c.Matched == distinctOptionValueIds.Length))
            {
                return Result.Failure(ProductVariantErrors.DuplicateCombination());
            }
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
                .Distinct()
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
                    ProductId = command.ProductId,
                    ProductVariant = variant
                })
                .ToList();

            await db.ProductVariants.AddAsync(variant, cancellationToken);
            await db.ProductVariantOptionValues.AddRangeAsync(variantOptionValues, cancellationToken);
            await db.ProductImages.AddRangeAsync(variantImages, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred when creating a new product variant for product {ProductId} with SKU {Sku}.",
                command.ProductId, command.Sku);
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure(ProductVariantErrors.UnexpectedError());
        }
    }
}
