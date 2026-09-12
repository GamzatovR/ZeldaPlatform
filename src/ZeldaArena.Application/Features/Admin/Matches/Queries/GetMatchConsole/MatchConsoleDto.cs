using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

/// <summary>
/// Состояние пульта. Оно же уходит в ответе на изменение счёта, поэтому клиент
/// перерисовывает пульт из ответа сервера и не считает счёт сам.
/// </summary>
public sealed record MatchConsoleDto
{
    public Guid Id { get; init; }

    public Guid TournamentId { get; init; }

    public string TournamentName { get; init; } = string.Empty;

    public string TournamentSlug { get; init; } = string.Empty;

    public string TeamAName { get; init; } = string.Empty;

    public string TeamBName { get; init; } = string.Empty;

    public int ScoreA { get; init; }

    public int ScoreB { get; init; }

    public int BestOf { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public MatchStatus Status { get; init; }

    public string? StreamUrl { get; init; }

    public int PlayerStatsCount { get; init; }

    /// <summary>Побед до конца серии: по нему пульт знает, когда матч можно завершить.</summary>
    public int WinsRequired => (BestOf / 2) + 1;

    public bool IsLive => Status == MatchStatus.Live;

    public bool CanStart => Status is MatchStatus.Scheduled or MatchStatus.Postponed;

    public bool CanFinish => IsLive && (ScoreA == WinsRequired || ScoreB == WinsRequired);

    /// <summary>
    /// Удалить можно только матч, которого «не было»: не начинался и без статистики
    /// (docs/adr/ADR-0010). Всё остальное отменяется.
    /// </summary>
    public bool CanDelete => Status is MatchStatus.Scheduled or MatchStatus.Postponed
        && ScoreA == 0 && ScoreB == 0 && PlayerStatsCount == 0;
}