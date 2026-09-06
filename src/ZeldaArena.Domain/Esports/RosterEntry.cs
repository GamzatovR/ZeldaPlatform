using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Запись в составе команды: с какого по какое число игрок в ней выступал.
/// Уход из команды не удаляет запись, а закрывает её — иначе история матчей
/// потеряет смысл (docs/SPEC.md §6, «состав историчен»).
/// </summary>
public class RosterEntry : BaseEntity
{
    private RosterEntry()
    {
    }

    public Guid TeamId { get; private set; }

    public Guid PlayerId { get; private set; }

    public PlayerRole Role { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public DateTimeOffset? LeftAt { get; private set; }

    /// <summary>
    /// Вычисляется из <see cref="LeftAt"/> и в БД не хранится: дублирующий столбец
    /// нарушил бы нормализацию и мог бы разойтись с датой ухода (docs/erd.md).
    /// </summary>
    public bool IsActive => LeftAt is null;

    public Team? Team { get; private set; }

    public Player? Player { get; private set; }

    internal static RosterEntry Open(Guid teamId, Guid playerId, PlayerRole role, DateTimeOffset joinedAt)
    {
        InvariantViolationException.ThrowIf(
            teamId == Guid.Empty || playerId == Guid.Empty,
            "roster.empty_reference",
            "Запись состава требует и команду, и игрока.");

        return new RosterEntry
        {
            TeamId = teamId,
            PlayerId = playerId,
            Role = role,
            JoinedAt = joinedAt,
        };
    }

    internal void Close(DateTimeOffset leftAt)
    {
        InvariantViolationException.ThrowIf(
            !IsActive,
            "roster.already_closed",
            "Запись состава уже закрыта.");

        InvariantViolationException.ThrowIf(
            leftAt < JoinedAt,
            "roster.left_before_joined",
            "Дата ухода из команды не может быть раньше даты прихода.");

        LeftAt = leftAt;
    }

    internal void ChangeRole(PlayerRole role)
    {
        InvariantViolationException.ThrowIf(
            !IsActive,
            "roster.already_closed",
            "Нельзя менять амплуа в закрытой записи состава.");

        Role = role;
    }
}
