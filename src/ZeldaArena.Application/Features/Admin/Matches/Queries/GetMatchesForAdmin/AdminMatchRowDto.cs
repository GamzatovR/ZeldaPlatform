using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;

public sealed record AdminMatchRowDto
{
    public Guid Id { get; init; }

    public string TournamentName { get; init; } = string.Empty;

    public string TeamAName { get; init; } = string.Empty;

    public string TeamBName { get; init; } = string.Empty;

    public int ScoreA { get; init; }

    public int ScoreB { get; init; }

    public int BestOf { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public MatchStatus Status { get; init; }
}