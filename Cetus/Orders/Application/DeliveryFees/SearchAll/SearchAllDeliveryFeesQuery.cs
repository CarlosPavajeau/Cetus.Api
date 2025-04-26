using MediatR;

namespace Cetus.Orders.Application.DeliveryFees.SearchAll;

public sealed record SearchAllDeliveryFeesQuery : IRequest<IEnumerable<DeliveryFeeResponse>>;
