using Application.Abstractions.Messaging;

namespace Application.Orders.DeliveryFees.SearchAll;

public sealed record SearchAllDeliveryFeesQuery : IQuery<IEnumerable<DeliveryFeeResponse>>;
