namespace Domain.States;

public sealed class State
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public IEnumerable<City> Cities { get; set; } = new List<City>();
}
