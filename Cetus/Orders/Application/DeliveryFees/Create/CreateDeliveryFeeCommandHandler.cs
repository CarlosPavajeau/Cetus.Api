using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Orders.Application.DeliveryFees.Find;
using Cetus.Orders.Domain;
using MediatR;

namespace Cetus.Orders.Application.DeliveryFees.Create;

internal sealed class CreateDeliveryFeeCommandHandler : IRequestHandler<CreateDeliveryFeeCommand, DeliveryFeeResponse>
{
    private readonly CetusDbContext _context;

    public CreateDeliveryFeeCommandHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryFeeResponse> Handle(CreateDeliveryFeeCommand request, CancellationToken cancellationToken)
    {
        var deliveryFee = new DeliveryFee
        {
            CityId = request.CityId,
            Fee = request.Fee
        };

        await _context.DeliveryFees.AddAsync(deliveryFee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeliveryFeeResponse(deliveryFee.Id, deliveryFee.Fee);
    }
}
