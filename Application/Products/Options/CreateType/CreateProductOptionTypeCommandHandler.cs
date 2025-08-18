using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Options.CreateType;

internal sealed class CreateProductOptionTypeCommandHandler(IApplicationDbContext db, ITenantContext context)
    : ICommandHandler<CreateProductOptionTypeCommand>
{
    public async Task<Result> Handle(CreateProductOptionTypeCommand command, CancellationToken cancellationToken)
    {
        var productOptionType = new ProductOptionType
        {
            Name = command.Name,
            StoreId = context.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ProductOptionValues = command.Values.Select(value => new ProductOptionValue
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList()
        };

        await db.ProductOptionTypes.AddAsync(productOptionType, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
