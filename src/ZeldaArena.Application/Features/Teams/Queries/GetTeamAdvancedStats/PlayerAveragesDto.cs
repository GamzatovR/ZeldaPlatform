namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

/// <summary>Средние показатели игрока за матчи, сыгранные за команду.</summary>
public sealed record PlayerAveragesDto
{
    public Guid PlayerId { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public int Matches { get; init; }

    public double Kills { get; init; }

    public double Deaths { get; init; }

    public double Assists { get; init; }

    public double Damage { get; init; }

    public double Rating { get; init; }

    /// <summary>(K + A) / D; без смертей делится на единицу, а не на ноль.</summary>
    public double Kda => (Kills + Assists) / Math.Max(1d, Deaths);
}