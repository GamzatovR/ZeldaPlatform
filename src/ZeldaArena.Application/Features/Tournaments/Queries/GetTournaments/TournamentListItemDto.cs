using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

public sealed record TournamentListItemDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public TournamentTier Tier { get; init; }

    public Region Region { get; init; }

    public decimal PrizePoolAmount { get; init; }

    public string PrizePoolCurrency { get; init; } = string.Empty;

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public TournamentStatus Status { get; init; }

    public string? LogoPath { get; init; }

    public bool IsFeatured { get; init; }

    public int ParticipantCount { get; init; }
}