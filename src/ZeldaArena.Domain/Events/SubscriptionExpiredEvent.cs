using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>Подписка истекла или отозвана.</summary>
public sealed record SubscriptionExpiredEvent(
    Guid SubscriptionId,
    Guid UserId,
    Guid PlanId) : DomainEvent;