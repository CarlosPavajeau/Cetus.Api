using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;

namespace Cetus.Application.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly CetusDbContext _context;

    public DeleteProductCommandHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([request.Id], cancellationToken);
        if (product == null)
        {
            return false;
        }

        product.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
