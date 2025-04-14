using Cetus.Orders.Domain;

namespace Cetus.Orders.Application.Summary;

public record OrderSummaryResponse(Guid Id, OrderStatus Status, DateTime CreatedAt)
{
    public static OrderSummaryResponse FromOrder(Order order)
    {
        return new OrderSummaryResponse(order.Id, order.Status, order.CreatedAt);
    }
}
