using Cetus.Orders.Application.DeliveryFees.Find;
using MediatR;

namespace Cetus.Orders.Application.DeliveryFees.Create;

public sealed record CreateDeliveryFeeCommand(Guid CityId, decimal Fee) : IRequest<DeliveryFeeResponse>;
