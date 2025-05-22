using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.SearchAll;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Create;

internal sealed class CreateProductCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
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
