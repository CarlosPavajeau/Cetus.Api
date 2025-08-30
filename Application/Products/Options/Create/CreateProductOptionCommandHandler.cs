using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Options.Create;

internal sealed class CreateProductOptionCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateProductOptionCommand>
{
    public async Task<Result> Handle(CreateProductOptionCommand command, CancellationToken cancellationToken)
    {
        var productStoreId = await db.Products
            .Where(p => p.Id == command.ProductId)
            .Select(p => p.StoreId)
            .SingleOrDefaultAsync(cancellationToken);

        var optionTypeStoreId = await db.ProductOptionTypes
            .Where(t => t.Id == command.OptionTypeId)
            .Select(t => t.StoreId)
            .SingleOrDefaultAsync(cancellationToken);

        if (productStoreId == Guid.Empty)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId.ToString()));
        }

        if (optionTypeStoreId == Guid.Empty)
        {
            return Result.Failure(ProductOptionTypeErrors.NotFound(command.OptionTypeId));
        }

        if (productStoreId != optionTypeStoreId)
        {
            return Result.Failure(ProductOptionErrors.CrossStoreAssociation());
        }

        var exists = await db.ProductOptions
            .AsNoTracking()
            .AnyAsync(
                p => p.ProductId == command.ProductId && p.OptionTypeId == command.OptionTypeId,
                cancellationToken
            );

        if (exists)
        {
            return Result.Success();
        }

        var productOption = new ProductOption
        {
            ProductId = command.ProductId,
            OptionTypeId = command.OptionTypeId,
        };

        await db.ProductOptions.AddAsync(productOption, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
