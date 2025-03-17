using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Products.Application.SearchAll;
using Cetus.Products.Domain;
using MediatR;

namespace Cetus.Products.Application.Create;

internal sealed class CreateProductCommandHandler(CetusDbContext context)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Enabled = true,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId
        };

        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }
}
