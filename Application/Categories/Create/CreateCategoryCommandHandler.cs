using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Categories.SearchAll;
using Domain.Categories;
using SharedKernel;

namespace Application.Categories.Create;

internal sealed class CreateCategoryCommandHandler(IApplicationDbContext context, ITenantContext tenant)
    : ICommandHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StoreId = tenant.Id
        };

        await context.Categories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return CategoryResponse.FromCategory(category);
    }
}
