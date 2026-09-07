using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>
/// Подписка активирована или продлена. Обработчики выдают роль Premium (только для
/// отображения), сбрасывают кэш прав, шлют уведомление и письмо (docs/SPEC.md §7.5).
/// </summary>
public sealed record SubscriptionActivatedEvent(
    Guid SubscriptionId,
    Guid UserId,
    Guid PlanId,
    DateTimeOffset EndsAt) : DomainEvent;