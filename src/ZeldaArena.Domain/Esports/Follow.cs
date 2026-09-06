using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Подписка пользователя на команду, игрока или турнир — источник персональных
/// уведомлений «матч избранной команды начинается» (docs/SPEC.md §11).
/// Пара «пользователь + цель» уникальна: повторный Follow не создаёт дубль.
/// </summary>
public class Follow : BaseEntity, IAuditableEntity
{
    private Follow()
    {
    }

    public Guid UserId { get; private set; }

    public FollowTargetType TargetType { get; private set; }

    public Guid TargetId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Follow Create(Guid userId, FollowTargetType targetType, Guid targetId)
    {
        InvariantViolationException.ThrowIf(
            userId == Guid.Empty || targetId == Guid.Empty,
            "follow.empty_reference",
            "Подписке нужны и пользователь, и цель.");

        return new Follow
        {
            UserId = userId,
            TargetType = targetType,
            TargetId = targetId,
        };
    }
}
