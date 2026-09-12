namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

/// <summary>Показатели игрока за матч.</summary>
public sealed record PlayerMatchStatsDto
{
    public Guid PlayerId { get; init; }

    public Guid TeamId { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public int Kills { get; init; }

    public int Deaths { get; init; }

    public int Assists { get; init; }

    public int Damage { get; init; }

    public decimal Rating { get; init; }
}