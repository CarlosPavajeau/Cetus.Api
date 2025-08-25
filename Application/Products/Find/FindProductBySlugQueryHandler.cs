using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Options;
using Application.Products.Variants;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Find;

internal sealed class FindProductBySlugQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindProductBySlugQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(FindProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var baseProduct = await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(p => p.Slug == request.Slug && p.DeletedAt == null && p.Enabled)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Rating,
                p.ReviewsCount,
                p.CategoryId,
                CategoryName = p.Category!.Name,
                CategorySlug = p.Category!.Slug,
                p.Enabled,
                p.StoreId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (baseProduct is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFoundBySlug(request.Slug));
        }

        var variants = await context.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == baseProduct.Id && v.DeletedAt == null)
            .Select(v => new
            {
                v.Id,
                v.Sku,
                v.Price,
                v.StockQuantity,
                Images = v.Images
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => new ProductImageResponse(i.Id, i.ImageUrl, i.AltText, i.SortOrder))
                    .ToList()
            })
            .OrderBy(v => v.Price)
            .ThenBy(v => v.Id)
            .ToListAsync(cancellationToken);

        var variantIds = variants.Select(v => v.Id).ToList();
        var variantOptionValues = new Dictionary<long, List<VariantOptionValueResponse>>();

        if (variantIds.Count > 0)
        {
            var optionValues = await context.ProductVariantOptionValues
                .AsNoTracking()
                .Where(vov =>
                    variantIds.Contains(vov.VariantId) &&
                    vov.ProductOptionValue!.DeletedAt == null &&
                    vov.ProductOptionValue.ProductOptionType!.DeletedAt == null)
                .Select(vov => new
                {
                    vov.VariantId,
                    vov.OptionValueId,
                    vov.ProductOptionValue!.Value,
                    vov.ProductOptionValue.OptionTypeId,
                    OptionTypeName = vov.ProductOptionValue.ProductOptionType!.Name
                })
                .ToListAsync(cancellationToken);

            variantOptionValues = optionValues
                .GroupBy(ov => ov.VariantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.OptionTypeName)
                        .ThenBy(x => x.Value)
                        .Select(x => new VariantOptionValueResponse(
                            x.OptionValueId,
                            x.Value,
                            x.OptionTypeId,
                            x.OptionTypeName))
                        .ToList()
                );
        }

        var availableOptions = await context.ProductOptions
            .AsNoTracking()
            .Where(po =>
                po.ProductId == baseProduct.Id
                && po.ProductOptionType!.DeletedAt == null)
            .Select(po => new ProductOptionTypeResponse(
                po.OptionTypeId,
                po.ProductOptionType!.Name,
                po.ProductOptionType.ProductOptionValues
                    .Where(ov => ov.DeletedAt == null)
                    .OrderBy(ov => ov.Value)
                    .Select(ov => new ProductOptionTypeValueResponse(ov.Id, ov.Value))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        var finalVariants = variants.Select(v => new ProductVariantResponse(
            v.Id,
            v.Sku,
            v.Price,
            v.StockQuantity,
            v.Images,
            variantOptionValues.GetValueOrDefault(v.Id, [])
        )).ToList();

        var response = new ProductResponse(
            baseProduct.Id,
            baseProduct.Name,
            baseProduct.Slug,
            baseProduct.Description,
            0m, // Excluded as requested
            0, // Excluded as requested  
            null, // Excluded as requested
            [], // Excluded as requested
            baseProduct.Rating,
            baseProduct.ReviewsCount,
            baseProduct.CategoryId,
            baseProduct.CategoryName,
            baseProduct.CategorySlug,
            baseProduct.Enabled,
            baseProduct.StoreId,
            finalVariants,
            availableOptions
        );

        return response;
    }
}
