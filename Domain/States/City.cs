namespace Domain.States;

public sealed class City
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid StateId { get; set; }
    public State? State { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
