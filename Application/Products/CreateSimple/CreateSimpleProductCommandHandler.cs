using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Create;
using Application.Products.SearchAll;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.CreateSimple;

internal sealed class CreateSimpleProductCommandHandler(IApplicationDbContext db, ITenantContext tenant)
    : ICommandHandler<CreateSimpleProductCommand, ProductResponse>

{
    public async Task<Result<ProductResponse>> Handle(CreateSimpleProductCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedSku = command.Sku.Trim().ToLowerInvariant();
        var skuExists = await db.ProductVariants
            .AsNoTracking()
            .AnyAsync(v =>
                    v.Sku == normalizedSku &&
                    v.DeletedAt == null,
                cancellationToken);

        if (skuExists)
        {
            return Result.Failure<ProductResponse>(ProductVariantErrors.DuplicateSku(normalizedSku));
        }

        var productId = Guid.NewGuid();
        var slug = CreateProductCommandHandler.GenerateSlug(command.Name, productId);

        var product = new Product
        {
            Id = productId,
            Name = command.Name,
            Slug = slug,
            Description = command.Description,
            Enabled = true,
            CategoryId = command.CategoryId,
            StoreId = tenant.Id
        };

        var variant = new ProductVariant
        {
            Sku = normalizedSku,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            ProductId = productId,
            Enabled = true
        };

        var variantImages = command.Images
            .Select(image => new ProductImage
            {
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                SortOrder = image.SortOrder,
                ProductId = productId,
                ProductVariant = variant
            })
            .ToList();

        await db.Products.AddAsync(product, cancellationToken);
        await db.ProductVariants.AddAsync(variant, cancellationToken);
        await db.ProductImages.AddRangeAsync(variantImages, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }
}
