using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Orders.Application.DeliveryFees.SearchAll;

internal sealed class
    SearchAllDeliveryFeesQueryHandler : IRequestHandler<SearchAllDeliveryFeesQuery, IEnumerable<DeliveryFeeResponse>>
{
    private readonly CetusDbContext _context;

    public SearchAllDeliveryFeesQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeliveryFeeResponse>> Handle(SearchAllDeliveryFeesQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryFees = await _context.DeliveryFees
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(x => x!.State)
            .Where(x => x.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return deliveryFees.Select(DeliveryFeeResponse.FromDeliveryFee).ToList();
    }
}
