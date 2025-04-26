namespace Cetus.Orders.Application.DeliveryFees.SearchAll;

public sealed record DeliveryFeeResponse(Guid Id, decimal Fee, string State, string City)
{
    public static DeliveryFeeResponse FromDeliveryFee(Domain.DeliveryFee deliveryFee)
    {
        return new DeliveryFeeResponse(
            deliveryFee.Id,
            deliveryFee.Fee,
            deliveryFee.City?.State?.Name ?? string.Empty,
            deliveryFee.City?.Name ?? string.Empty
        );
    }
}
