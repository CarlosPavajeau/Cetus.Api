using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.DeliveryFees.Find;
using Domain.Orders;
using SharedKernel;

namespace Application.Orders.DeliveryFees.Create;

internal sealed class CreateDeliveryFeeCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateDeliveryFeeCommand, DeliveryFeeResponse>
{
    public async Task<Result<DeliveryFeeResponse>> Handle(CreateDeliveryFeeCommand request,
        CancellationToken cancellationToken)
    {
        var deliveryFee = new DeliveryFee
        {
            CityId = request.CityId,
            Fee = request.Fee
        };

        await context.DeliveryFees.AddAsync(deliveryFee, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new DeliveryFeeResponse(deliveryFee.Id, deliveryFee.Fee);
    }
}
