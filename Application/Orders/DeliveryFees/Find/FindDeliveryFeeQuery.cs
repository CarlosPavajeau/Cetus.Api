using Application.Abstractions.Messaging;

namespace Application.Orders.DeliveryFees.Find;

public sealed record FindDeliveryFeeQuery(Guid CityId) : IQuery<DeliveryFeeResponse>;
