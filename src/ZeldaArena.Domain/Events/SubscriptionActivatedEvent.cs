using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>Подписка активирована или продлена.</summary>
public sealed record SubscriptionActivatedEvent(
    Guid SubscriptionId,
    Guid UserId,
    Guid PlanId,
    DateTimeOffset EndsAt) : DomainEvent;