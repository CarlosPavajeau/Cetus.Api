using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.DomainEvents;

internal sealed class DomainEventsPooler(
    DomainEventsChannel channel,
    IServiceProvider serviceProvider,
    ILogger<DomainEventsPooler> logger)
    : BackgroundService
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await channel.WaitToRead(stoppingToken))
        {
            IDomainEvent @event;

            try
            {
                @event = await channel.Receive(stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to read domain event from channel.");
                continue;
            }

            using var scope = serviceProvider.CreateScope();

            var domainEventType = @event.GetType();
            var handlerType = HandlerTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(IDomainEventHandler<>).MakeGenericType(et));

            var handlers = scope.ServiceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null)
                {
                    continue;
                }

                var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);

                try
                {
                    await handlerWrapper.Handle(@event, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Domain event handler {Handler} failed for {EventType}.",
                        handler.GetType().FullName, domainEventType.FullName);
                }
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            var wrapperType = WrapperTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));

            var instance = Activator.CreateInstance(wrapperType, handler);

            if (instance is null)
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of {wrapperType} for handler {handler.GetType().FullName}.");
            }

            return (HandlerWrapper) instance;
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>) handler;

        public override async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _handler.Handle((T) domainEvent, cancellationToken);
        }
    }
}
