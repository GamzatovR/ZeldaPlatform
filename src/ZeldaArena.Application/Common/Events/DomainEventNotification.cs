using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Events;

/// <summary>
/// Обёртка доменного события в уведомление MediatR. Нужна ровно затем, чтобы
/// <see cref="DomainEvent"/> не наследовал <c>INotification</c>: Domain остаётся
/// на голом BCL (docs/SPEC.md §5.2), а знание о шине сообщений живёт здесь.
///
/// Обработчик пишется на конкретное событие:
/// <c>INotificationHandler&lt;DomainEventNotification&lt;MatchScoreChangedEvent&gt;&gt;</c>.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : DomainEvent;