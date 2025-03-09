using Cetus.Application.SearchAllProducts;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Application.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse?>
{
    private readonly CetusDbContext _dbContext;

    public UpdateProductCommandHandler(CetusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductResponse?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FindAsync([request.Id], cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Enabled = request.Enabled;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }
}
