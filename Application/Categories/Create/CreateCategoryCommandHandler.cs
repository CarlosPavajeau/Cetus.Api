using System.Text.RegularExpressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Categories.SearchAll;
using Domain.Categories;
using SharedKernel;

namespace Application.Categories.Create;

internal sealed partial class CreateCategoryCommandHandler(IApplicationDbContext context, ITenantContext tenant)
    : ICommandHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = request.Name,
            Slug = GenerateSlug(categoryId, request.Name, tenant.Id),
            StoreId = tenant.Id
        };

        await context.Categories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return CategoryResponse.FromCategory(category);
    }

    private static string GenerateSlug(Guid id, string name, Guid storeId)
    {
        var baseSlug = CategoryNameRegex().Replace(name.ToLower(), "-");
        
        var idSuffix = id.ToString()[(id.ToString().Length - 4)..];
        var storeIdSuffix = storeId.ToString()[(storeId.ToString().Length - 4)..];
        
        return SlugRegex().Replace($"{baseSlug}-{idSuffix}-{storeIdSuffix}", "-");
    }
    
    [GeneratedRegex("[^a-z0-9]")]
    private static partial Regex CategoryNameRegex();

    [GeneratedRegex("-+")]
    private static partial Regex SlugRegex();
}
