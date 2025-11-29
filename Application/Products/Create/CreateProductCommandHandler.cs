using System.Text.RegularExpressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Create;

internal sealed partial class CreateProductCommandHandler(IApplicationDbContext context, ITenantContext tenant)
    : ICommandHandler<CreateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var productId = Guid.NewGuid();
        string slug = GenerateSlug(request.Name, productId);

        var product = new Product
        {
            Id = productId,
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Enabled = true,
            CategoryId = request.CategoryId,
            StoreId = tenant.Id
        };

        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }

    internal static string GenerateSlug(string name, Guid id)
    {
        // Convert name to lowercase and replace non-alphanumeric chars with hyphens
#pragma warning disable CA1308 // TODO: Replace with ToUpper
        string baseSlug = ProductNameRegex().Replace(name.ToLowerInvariant(), "-");

        // Get last 4 chars of the ID
        string idSuffix = id.ToString("N")[^8..];

        // Combine and ensure no double hyphens
        string combined = $"{baseSlug}-{idSuffix}";

        return SlugRegex().Replace(combined, "-").Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]")]
    private static partial Regex ProductNameRegex();

    [GeneratedRegex("-+")]
    private static partial Regex SlugRegex();
}
