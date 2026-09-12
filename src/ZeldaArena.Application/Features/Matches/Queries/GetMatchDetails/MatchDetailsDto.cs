using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

public sealed record MatchDetailsDto
{
    public required MatchCardDto Card { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? EndedAt { get; init; }

    public string? StreamUrl { get; init; }

    public IReadOnlyList<LineupPlayerDto> LineupA { get; init; } = [];

    public IReadOnlyList<LineupPlayerDto> LineupB { get; init; } = [];

    public IReadOnlyList<PlayerMatchStatsDto> StatsA { get; init; } = [];

    public IReadOnlyList<PlayerMatchStatsDto> StatsB { get; init; } = [];
}