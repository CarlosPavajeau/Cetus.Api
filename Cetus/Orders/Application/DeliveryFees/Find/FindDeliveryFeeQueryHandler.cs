using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Orders.Application.DeliveryFees.Find;

internal sealed class FindDeliveryFeeQueryHandler(CetusDbContext context)
    : IRequestHandler<FindDeliveryFeeQuery, DeliveryFeeResponse>
{
    public async Task<DeliveryFeeResponse> Handle(FindDeliveryFeeQuery request, CancellationToken cancellationToken)
    {
        var deliveryFee = await context.DeliveryFees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CityId == request.CityId, cancellationToken);

        return deliveryFee == null
            ? DeliveryFeeResponse.Empty
            : new DeliveryFeeResponse(deliveryFee.Id, deliveryFee.Fee);
    }
}
