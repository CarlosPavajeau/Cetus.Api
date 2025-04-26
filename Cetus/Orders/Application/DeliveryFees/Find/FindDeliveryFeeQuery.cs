using MediatR;

namespace Cetus.Orders.Application.DeliveryFees.Find;

public sealed record FindDeliveryFeeQuery(Guid CityId) : IRequest<DeliveryFeeResponse>;
