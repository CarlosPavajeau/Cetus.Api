namespace Domain.Orders;

public class OrderTimeline
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }

    public string? ChangedById { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
