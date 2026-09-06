using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Показатели игрока в конкретном матче. Источник расширенной статистики,
/// закрытой фичей stats.advanced (docs/SPEC.md §8.2).
/// </summary>
public class PlayerMatchStats : BaseEntity
{
    private PlayerMatchStats()
    {
    }

    public Guid MatchId { get; private set; }

    public Guid PlayerId { get; private set; }

    public Guid TeamId { get; private set; }

    public int Kills { get; private set; }

    public int Deaths { get; private set; }

    public int Assists { get; private set; }

    public int Damage { get; private set; }

    /// <summary>Сводный рейтинг за матч, обычно около 1.00.</summary>
    public decimal Rating { get; private set; }

    public Match? Match { get; private set; }

    public Player? Player { get; private set; }

    public Team? Team { get; private set; }

    internal static PlayerMatchStats Create(
        Guid matchId,
        Guid playerId,
        Guid teamId,
        int kills,
        int deaths,
        int assists,
        int damage,
        decimal rating)
    {
        InvariantViolationException.ThrowIf(
            kills < 0 || deaths < 0 || assists < 0 || damage < 0,
            "player_stats.negative_value",
            "Показатели матча не могут быть отрицательными.");

        InvariantViolationException.ThrowIf(
            rating < 0m,
            "player_stats.negative_rating",
            "Рейтинг за матч не может быть отрицательным.");

        return new PlayerMatchStats
        {
            MatchId = matchId,
            PlayerId = playerId,
            TeamId = teamId,
            Kills = kills,
            Deaths = deaths,
            Assists = assists,
            Damage = damage,
            Rating = decimal.Round(rating, 2),
        };
    }
}