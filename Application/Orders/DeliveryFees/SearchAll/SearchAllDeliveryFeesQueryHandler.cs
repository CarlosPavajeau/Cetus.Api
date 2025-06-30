using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.DeliveryFees.SearchAll;

internal sealed class SearchAllDeliveryFeesQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<SearchAllDeliveryFeesQuery, IEnumerable<DeliveryFeeResponse>>
{
    public async Task<Result<IEnumerable<DeliveryFeeResponse>>> Handle(SearchAllDeliveryFeesQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryFees = await context.DeliveryFees
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(x => x!.State)
            .Where(x => x.DeletedAt == null && x.StoreId == tenant.Id)
            .ToListAsync(cancellationToken);

        return deliveryFees.Select(DeliveryFeeResponse.FromDeliveryFee).ToList();
    }
}
