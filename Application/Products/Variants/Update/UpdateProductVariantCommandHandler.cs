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
        var productVariant = await db.ProductVariants
            .Where(p => p.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (productVariant is null)
        {
            return Result.Failure<SimpleProductVariantResponse>(ProductVariantErrors.NotFound(command.Id));
        }

        productVariant.Price = command.Price;
        productVariant.Enabled = command.Enabled;
        productVariant.Featured = command.Featured;

        await db.SaveChangesAsync(cancellationToken);

        return SimpleProductVariantResponse.From(productVariant);
    }
}
