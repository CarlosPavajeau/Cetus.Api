using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.SearchAll;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
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
        
        // Delete all product images
        await db.ProductImages
            .Where(p => p.ProductId == product.Id)
            .ExecuteDeleteAsync(cancellationToken);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.Enabled = request.Enabled;
        
        var images = request.Images.Select(img => new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = img.ImageUrl,
            AltText = img.AltText,
            SortOrder = img.SortOrder
        }).ToList();

        product.Images = images;

        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromProduct(product);
    }
}
