namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

public sealed record TeamAdvancedStatsDto
{
    public IReadOnlyList<PlayerAveragesDto> Players { get; init; } = [];

    public IReadOnlyList<TournamentRecordDto> Tournaments { get; init; } = [];
}