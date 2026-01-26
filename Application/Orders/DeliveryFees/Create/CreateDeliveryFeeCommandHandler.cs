using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.DeliveryFees.Find;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.DeliveryFees.Create;

internal sealed class CreateDeliveryFeeCommandHandler(IApplicationDbContext context, ITenantContext tenant)
    : ICommandHandler<CreateDeliveryFeeCommand, DeliveryFeeResponse>
{
    public async Task<Result<DeliveryFeeResponse>> Handle(CreateDeliveryFeeCommand request,
        CancellationToken cancellationToken)
    {
        bool alreadyExists = await context.DeliveryFees
            .AnyAsync(df => df.CityId == request.CityId && df.StoreId == tenant.Id, cancellationToken);

        if (alreadyExists)
        {
            return Result.Failure<DeliveryFeeResponse>(DeliveryFeeErrors.AlreadyExists(request.CityId));
        }

        var deliveryFee = new DeliveryFee
        {
            CityId = request.CityId,
            Fee = request.Fee,
            StoreId = tenant.Id
        };

        await context.DeliveryFees.AddAsync(deliveryFee, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new DeliveryFeeResponse(deliveryFee.Id, deliveryFee.Fee);
    }
}
