using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.DeliveryFees.Find;

internal sealed class FindDeliveryFeeQueryHandler(IApplicationDbContext context)
    : IQueryHandler<FindDeliveryFeeQuery, DeliveryFeeResponse>
{
    public async Task<Result<DeliveryFeeResponse>> Handle(FindDeliveryFeeQuery request,
        CancellationToken cancellationToken)
    {
        var deliveryFee = await context.DeliveryFees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CityId == request.CityId, cancellationToken);

        return deliveryFee == null
            ? DeliveryFeeResponse.Empty
            : new DeliveryFeeResponse(deliveryFee.Id, deliveryFee.Fee);
    }
}
