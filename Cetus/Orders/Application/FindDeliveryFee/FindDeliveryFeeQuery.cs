using MediatR;

namespace Cetus.Orders.Application.FindDeliveryFee;

public sealed record FindDeliveryFeeQuery(Guid CityId) : IRequest<DeliveryFeeResponse>;
