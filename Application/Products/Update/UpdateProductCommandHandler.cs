using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.SearchAll;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Update;

internal sealed class UpdateProductCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateProductCommand, ProductResponse?>
{
    public async Task<Result<ProductResponse?>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FindAsync([request.Id], cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductResponse?>(ProductErrors.NotFound(request.Id.ToString()));
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.Enabled = request.Enabled;

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            product.ImageUrl = request.ImageUrl;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }
}
