using System.ComponentModel.DataAnnotations;

namespace Cetus.Domain;

public class State
{
    public Guid Id { get; set; }

    [MaxLength(256)] public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public IEnumerable<City> Cities { get; set; } = new List<City>();
}
