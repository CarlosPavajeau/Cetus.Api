namespace Application.Orders.DeliveryFees.Find;

public record DeliveryFeeResponse(Guid Id, decimal Fee)
{
    private const decimal DefaultFee = 15000.0M;
    public static readonly DeliveryFeeResponse Empty = new(Guid.Empty, DefaultFee);
}
