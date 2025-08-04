using System.Text.RegularExpressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.SearchAll;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Create;

internal sealed partial class CreateProductCommandHandler(IApplicationDbContext context, ITenantContext tenant)
    : ICommandHandler<CreateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var productId = Guid.NewGuid();
        var slug = GenerateSlug(request.Name, productId);

        var images = request.Images.Select(img => new ProductImage
        {
            ProductId = productId,
            ImageUrl = img.ImageUrl,
            AltText = img.AltText,
            SortOrder = img.SortOrder
        });

        var product = new Product
        {
            Id = productId,
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Enabled = true,
            Images = images,
            CategoryId = request.CategoryId,
            StoreId = tenant.Id
        };

        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }

    private static string GenerateSlug(string name, Guid id)
    {
        // Convert name to lowercase and replace non-alphanumeric chars with hyphens
        var baseSlug = ProductNameRegex().Replace(name.ToLower(), "-");

        // Get last 4 chars of the ID
        var idSuffix = id.ToString()[(id.ToString().Length - 4)..];

        // Combine and ensure no double hyphens
        return SlugRegex().Replace($"{baseSlug}-{idSuffix}", "-");
    }

    [GeneratedRegex("[^a-z0-9]")]
    private static partial Regex ProductNameRegex();

    [GeneratedRegex("-+")]
    private static partial Regex SlugRegex();
}
