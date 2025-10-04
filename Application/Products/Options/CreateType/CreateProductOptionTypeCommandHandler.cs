using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Options.CreateType;

internal sealed class CreateProductOptionTypeCommandHandler(
    IApplicationDbContext db,
    ITenantContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateProductOptionTypeCommand>
{
    public async Task<Result> Handle(CreateProductOptionTypeCommand command, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var normalizedName = command.Name.Trim();

        var normalizedValues = command.Values
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var productOptionType = new ProductOptionType
        {
            Name = normalizedName,
            StoreId = context.Id,
            CreatedAt = now,
            UpdatedAt = now,
            ProductOptionValues = normalizedValues.Select(value => new ProductOptionValue
            {
                Value = value,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList()
        };

        db.ProductOptionTypes.Add(productOptionType);

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
