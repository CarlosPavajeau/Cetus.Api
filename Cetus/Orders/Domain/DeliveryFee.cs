using System.ComponentModel.DataAnnotations;
using Cetus.States.Domain;

namespace Cetus.Orders.Domain;

public class DeliveryFee
{
    public Guid Id { get; set; }
    public decimal Fee { get; set; }

    public Guid CityId { get; set; }
    public City? City { get; set; }

    [Required] [MaxLength(64)] public string OrganizationId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
