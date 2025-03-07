using MediatR;

namespace Cetus.Shared.Domain;

public abstract record DomainEvent : INotification
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
