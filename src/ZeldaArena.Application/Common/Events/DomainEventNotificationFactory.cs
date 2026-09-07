using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Events;

/// <summary>
/// Собирает <see cref="DomainEventNotification{TDomainEvent}"/> по фактическому типу
/// события. Диспетчер в Infrastructure видит события только как <see cref="DomainEvent"/>
/// и подобрать нужный обобщённый тип сам не может — знание о том, во что заворачивать,
/// остаётся в Application.
/// </summary>
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