using Domain.States;

namespace Domain.Orders;

public sealed class DeliveryFee
{
    public Guid Id { get; set; }
    public decimal Fee { get; set; }

    public Guid CityId { get; set; }
    public City? City { get; set; }

    public Guid StoreId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
