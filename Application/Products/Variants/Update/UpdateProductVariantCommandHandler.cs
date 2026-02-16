using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Variants.Update;

internal sealed class UpdateProductVariantCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateProductVariantCommand, SimpleProductVariantResponse>
{
    public async Task<Result<SimpleProductVariantResponse>> Handle(UpdateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants
            .Where(p => p.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return Result.Failure<SimpleProductVariantResponse>(ProductVariantErrors.NotFound(command.Id));
        }

        variant.Price = command.Price;
        variant.RetailPrice = command.RetailPrice;
        variant.CompareAtPrice = command.CompareAtPrice;
        variant.Enabled = command.Enabled;
        variant.Featured = command.Featured;

        await db.SaveChangesAsync(cancellationToken);

        return SimpleProductVariantResponse.From(variant);
    }
}
