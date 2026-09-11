namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;

/// <summary>Средние показатели игрока в одном турнире.</summary>
public sealed record PlayerTournamentStatsDto
{
    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int Matches { get; init; }

    public double Kills { get; init; }

    public double Deaths { get; init; }

    public double Assists { get; init; }

    public double Damage { get; init; }

    public double Rating { get; init; }

    public double Kda => (Kills + Assists) / Math.Max(1d, Deaths);
}