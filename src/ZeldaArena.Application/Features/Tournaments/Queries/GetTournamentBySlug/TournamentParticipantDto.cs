namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

/// <summary>Команда-участница с рейтингом.</summary>
public sealed record TournamentParticipantDto
{
    public Guid TeamId { get; init; }

    public string TeamSlug { get; init; } = string.Empty;

    public string TeamName { get; init; } = string.Empty;

    public string TeamTag { get; init; } = string.Empty;

    public string? TeamLogoPath { get; init; }

    public int Rating { get; init; }

    public int Seed { get; init; }

    public int? Placement { get; init; }
}