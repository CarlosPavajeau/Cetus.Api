using MediatR;

namespace Cetus.Orders.Application.DeliveryFee.Find;

public sealed record FindDeliveryFeeQuery(Guid CityId) : IRequest<DeliveryFeeResponse>;
