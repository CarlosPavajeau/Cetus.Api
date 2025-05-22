using Domain.Orders;

namespace Application.Orders.Summary;

public record OrderSummaryResponse(Guid Id, OrderStatus Status, DateTime CreatedAt)
{
    public static OrderSummaryResponse FromOrder(Order order)
    {
        return new OrderSummaryResponse(order.Id, order.Status, order.CreatedAt);
    }
}
