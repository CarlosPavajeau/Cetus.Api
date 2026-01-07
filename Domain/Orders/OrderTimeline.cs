namespace Domain.Orders;

public class OrderTimeline
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }

    public OrderStatus? FromStatus { get; init; }
    public OrderStatus ToStatus { get; init; }

    public string? ChangedById { get; init; }
    public string? Notes { get; init; }

    public DateTime CreatedAt { get; init; }
}
