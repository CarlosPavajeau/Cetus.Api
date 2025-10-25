using System.Threading.Channels;
using SharedKernel;

namespace Infrastructure.DomainEvents;

internal sealed class DomainEventsChannel
{
    private static readonly BoundedChannelOptions DefaultOptions = new(capacity: 10_000)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait,
        AllowSynchronousContinuations = true
    };

    private readonly Channel<IDomainEvent> _channel = Channel.CreateBounded<IDomainEvent>(DefaultOptions);

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
