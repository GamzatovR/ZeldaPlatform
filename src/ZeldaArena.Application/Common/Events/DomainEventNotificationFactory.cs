using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Events;

public static class DomainEventNotificationFactory
{
    public static INotification Create(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var notificationType = typeof(DomainEventNotification<>)
            .MakeGenericType(domainEvent.GetType());

        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }
}