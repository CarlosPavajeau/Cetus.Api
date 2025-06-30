using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.DeliveryFees.Find;

internal sealed class FindDeliveryFeeQueryHandler(IApplicationDbContext context, ITenantContext tenant)
    : IQueryHandler<FindDeliveryFeeQuery, DeliveryFeeResponse>
{
    public async Task<Result<DeliveryFeeResponse>> Handle(FindDeliveryFeeQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryFee = await context.DeliveryFees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CityId == request.CityId && x.StoreId == tenant.Id, cancellationToken);

        if (deliveryFee is null)
        {
            return DeliveryFeeResponse.Empty;
        }

        return new DeliveryFeeResponse(deliveryFee.Id, deliveryFee.Fee);
    }
}
