using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.SearchAll;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Update;

internal sealed class UpdateProductCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([request.Id], cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Id.ToString()));
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.Enabled = request.Enabled;

        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }
}
