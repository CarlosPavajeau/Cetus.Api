using Application.Abstractions.Messaging;
using Application.Orders.DeliveryFees.Find;

namespace Application.Orders.DeliveryFees.Create;

public sealed record CreateDeliveryFeeCommand(Guid CityId, decimal Fee) : ICommand<DeliveryFeeResponse>;
