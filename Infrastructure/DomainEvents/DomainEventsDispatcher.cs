using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.DomainEvents;

public sealed class DomainEventsDispatcher(IServiceProvider serviceProvider)
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = (IEnumerable<object>) scope.ServiceProvider.GetService(
                typeof(IEnumerable<>).MakeGenericType(handlerType));

            foreach (object handler in handlers ?? [])
            {
                MethodInfo? handleMethod = handlerType.GetMethod("Handle");
                if (handleMethod != null)
                {
                    await (Task) handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                }
            }
        }
    }
}
