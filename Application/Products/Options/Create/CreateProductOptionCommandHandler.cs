using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Options.Create;

internal sealed class CreateProductOptionCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateProductOptionCommand>
{
    public async Task<Result> Handle(CreateProductOptionCommand command, CancellationToken cancellationToken)
    {
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
