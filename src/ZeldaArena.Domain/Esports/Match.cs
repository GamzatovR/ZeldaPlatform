using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Матч турнира — главная сущность проекта: его счёт правит модератор в пульте,
/// а зрители видят изменение мгновенно (docs/SPEC.md §11).
/// Все переходы состояния идут только через методы этого класса.
/// </summary>
public class Match : BaseEntity, IAuditableEntity
{
    private readonly List<PlayerMatchStats> _playerStats = [];

    private Match()
    {
    }

    public Guid TournamentId { get; private set; }

    public Guid TeamAId { get; private set; }

    public Guid TeamBId { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public MatchStatus Status { get; private set; }

    /// <summary>Формат серии: нечётное число карт (Bo1, Bo3, Bo5).</summary>
    public int BestOf { get; private set; }

    public int ScoreA { get; private set; }

    public int ScoreB { get; private set; }

    public Guid? WinnerTeamId { get; private set; }

    public string? StreamUrl { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Tournament? Tournament { get; private set; }

    public Team? TeamA { get; private set; }

    public Team? TeamB { get; private set; }

    public IReadOnlyCollection<PlayerMatchStats> PlayerStats => _playerStats.AsReadOnly();

    /// <summary>Побед для выигрыша серии: в Bo3 — две, в Bo5 — три.</summary>
    public int WinsRequired => (BestOf / 2) + 1;

    public bool IsFinished => Status == MatchStatus.Finished;

    public static Match Schedule(
        Guid tournamentId,
        Guid teamAId,
        Guid teamBId,
        DateTimeOffset scheduledAt,
        int bestOf,
        string? streamUrl = null)
    {
        InvariantViolationException.ThrowIf(
            tournamentId == Guid.Empty || teamAId == Guid.Empty || teamBId == Guid.Empty,
            "match.empty_reference",
            "Матчу нужны турнир и обе команды.");

        InvariantViolationException.ThrowIf(
            teamAId == teamBId,
            "match.same_team",
            "Команда не может играть сама с собой.");

        InvariantViolationException.ThrowIf(
            bestOf < 1 || bestOf % 2 == 0,
            "match.invalid_best_of",
            $"Формат матча — нечётное число карт, получено {bestOf}.");

        return new Match
        {
            TournamentId = tournamentId,
            TeamAId = teamAId,
            TeamBId = teamBId,
            ScheduledAt = scheduledAt,
            BestOf = bestOf,
            Status = MatchStatus.Scheduled,
            StreamUrl = string.IsNullOrWhiteSpace(streamUrl) ? null : streamUrl.Trim(),
        };
    }

    public void Start(DateTimeOffset startedAt)
    {
        InvariantViolationException.ThrowIf(
            Status is not (MatchStatus.Scheduled or MatchStatus.Postponed),
            "match.cannot_start",
            $"Начать можно только запланированный матч, текущий статус — {Status}.");

        Status = MatchStatus.Live;
        StartedAt = startedAt;
    }

    /// <summary>
    /// Единственный способ изменить счёт (CLAUDE.md). Завершённый матч не редактируется,
    /// счёт не может выйти за формат серии, каждое изменение порождает доменное событие.
    /// </summary>
    public void UpdateScore(int scoreA, int scoreB)
    {
        InvariantViolationException.ThrowIf(
            IsFinished,
            "match.finished_is_read_only",
            "Счёт завершённого матча изменить нельзя.");

        InvariantViolationException.ThrowIf(
            Status != MatchStatus.Live,
            "match.not_live",
            $"Счёт правится только у идущего матча, текущий статус — {Status}.");

        InvariantViolationException.ThrowIf(
            scoreA < 0 || scoreB < 0,
            "match.negative_score",
            "Счёт не может быть отрицательным.");

        InvariantViolationException.ThrowIf(
            scoreA > WinsRequired || scoreB > WinsRequired,
            "match.score_exceeds_wins_required",
            $"В формате Bo{BestOf} команда не может выиграть больше {WinsRequired} карт.");

        // Сумма отсекает и вторую победу подряд: при любом нечётном BestOf
        // счёт, где обе команды набрали WinsRequired, выходит за формат серии.
        InvariantViolationException.ThrowIf(
            scoreA + scoreB > BestOf,
            "match.score_exceeds_best_of",
            $"Сумма счетов не может превышать {BestOf}.");

        // Повторная отправка того же счёта не должна поднимать зрителям лишнее событие.
        if (scoreA == ScoreA && scoreB == ScoreB)
        {
            return;
        }

        ScoreA = scoreA;
        ScoreB = scoreB;

        Raise(new MatchScoreChangedEvent(Id, TournamentId, ScoreA, ScoreB));
    }

    /// <summary>
    /// Завершает серию. Победитель выводится из счёта, а не приходит извне,
    /// поэтому таблица результатов не может разойтись со счётом.
    /// </summary>
    public void Finish(DateTimeOffset endedAt)
    {
        InvariantViolationException.ThrowIf(
            Status != MatchStatus.Live,
            "match.not_live",
            $"Завершить можно только идущий матч, текущий статус — {Status}.");

        InvariantViolationException.ThrowIf(
            ScoreA != WinsRequired && ScoreB != WinsRequired,
            "match.no_winner_yet",
            $"Ни одна команда ещё не набрала {WinsRequired} побед.");

        Status = MatchStatus.Finished;
        EndedAt = endedAt;
        WinnerTeamId = ScoreA > ScoreB ? TeamAId : TeamBId;

        Raise(new MatchFinishedEvent(Id, TournamentId, WinnerTeamId.Value, ScoreA, ScoreB));
    }

    public void Postpone(DateTimeOffset newScheduledAt)
    {
        InvariantViolationException.ThrowIf(
            Status is not (MatchStatus.Scheduled or MatchStatus.Postponed),
            "match.cannot_postpone",
            $"Перенести можно только запланированный матч, текущий статус — {Status}.");

        Status = MatchStatus.Postponed;
        ScheduledAt = newScheduledAt;
    }

    public void Cancel()
    {
        InvariantViolationException.ThrowIf(
            IsFinished,
            "match.finished_is_read_only",
            "Завершённый матч отменить нельзя.");

        Status = MatchStatus.Canceled;
    }

    public void SetStreamUrl(string? streamUrl) =>
        StreamUrl = string.IsNullOrWhiteSpace(streamUrl) ? null : streamUrl.Trim();

    public void Reschedule(DateTimeOffset scheduledAt)
    {
        InvariantViolationException.ThrowIf(
            Status != MatchStatus.Scheduled,
            "match.cannot_reschedule",
            $"Изменить время можно только у запланированного матча, текущий статус — {Status}.");

        ScheduledAt = scheduledAt;
    }

    public PlayerMatchStats AddPlayerStats(
        Guid playerId,
        Guid teamId,
        int kills,
        int deaths,
        int assists,
        int damage,
        decimal rating)
    {
        InvariantViolationException.ThrowIf(
            teamId != TeamAId && teamId != TeamBId,
            "match.foreign_team",
            "Статистику можно записать только игроку одной из команд матча.");

        InvariantViolationException.ThrowIf(
            _playerStats.Any(stats => stats.PlayerId == playerId),
            "match.duplicate_player_stats",
            "Статистика этого игрока в матче уже записана.");

        var playerStats = PlayerMatchStats.Create(Id, playerId, teamId, kills, deaths, assists, damage, rating);
        _playerStats.Add(playerStats);
        return playerStats;
    }
}