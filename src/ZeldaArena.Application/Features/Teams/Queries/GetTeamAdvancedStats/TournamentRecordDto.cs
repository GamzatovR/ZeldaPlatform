namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

/// <summary>Результаты в одном турнире: сыграно и выиграно.</summary>
public sealed record TournamentRecordDto
{
    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int Played { get; init; }

    public int Wins { get; init; }

    public int WinRate => Played == 0 ? 0 : (int)Math.Round(Wins * 100m / Played);
}