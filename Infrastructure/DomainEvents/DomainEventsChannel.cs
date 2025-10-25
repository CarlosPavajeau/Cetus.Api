using System.Threading.Channels;
using SharedKernel;

namespace Infrastructure.DomainEvents;

internal sealed class DomainEventsChannel
{
    private readonly Channel<IDomainEvent> _channel = Channel.CreateUnbounded<IDomainEvent>();

    public ValueTask Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(domainEvent, cancellationToken);
    }

    public ValueTask<IDomainEvent> Receive(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    public ValueTask<bool> WaitToRead(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.WaitToReadAsync(cancellationToken);
    }
}
