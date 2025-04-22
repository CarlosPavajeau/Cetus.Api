namespace Cetus.Orders.Application.FindDeliveryFee;

public record DeliveryFeeResponse(Guid Id, decimal Fee)
{
    private const decimal DefaultFee = 10000.0M;
    public static readonly DeliveryFeeResponse Empty = new(Guid.Empty, DefaultFee);
}
