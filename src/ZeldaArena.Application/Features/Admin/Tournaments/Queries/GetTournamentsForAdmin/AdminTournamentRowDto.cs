using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

public sealed record AdminTournamentRowDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public TournamentTier Tier { get; init; }

    public Region Region { get; init; }

    public TournamentStatus Status { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public decimal PrizePool { get; init; }

    public string Currency { get; init; } = string.Empty;

    public bool IsFeatured { get; init; }

    public int ParticipantCount { get; init; }

    public int MatchCount { get; init; }
}