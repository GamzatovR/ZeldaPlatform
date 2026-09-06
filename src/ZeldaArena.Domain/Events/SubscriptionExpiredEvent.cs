using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>
/// Подписка истекла или отозвана. Роль Premium снимается, кэш прав сбрасывается.
/// Команда пользователя при этом сохраняется, но её редактирование блокируется
/// отсутствием фичи team.create (CLAUDE.md, «При неопределённости»).
/// </summary>
public sealed record SubscriptionExpiredEvent(
    Guid SubscriptionId,
    Guid UserId,
    Guid PlanId) : DomainEvent;
