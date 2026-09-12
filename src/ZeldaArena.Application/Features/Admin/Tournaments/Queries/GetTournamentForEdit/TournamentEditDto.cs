using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

public sealed record TournamentEditDto
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

    public string? Description { get; init; }

    public string? RulesHtml { get; init; }

    public string? LogoPath { get; init; }

    public bool IsFeatured { get; init; }

    public int MatchCount { get; init; }

    public IReadOnlyList<TournamentParticipantDto> Participants { get; init; } = [];

    /// <summary>Одобренные команды, которых ещё нет среди участников.</summary>
    public IReadOnlyList<TeamOptionDto> AvailableTeams { get; init; } = [];

    /// <summary>Состав закрыт для завершённого и отменённого турнира (инвариант <c>Tournament</c>).</summary>
    public bool IsRosterOpen => Status is TournamentStatus.Announced or TournamentStatus.Ongoing;

    /// <summary>Следующий свободный посев — подсказка для формы добавления участника.</summary>
    public int NextSeed => Participants.Count == 0 ? 1 : Participants.Max(participant => participant.Seed) + 1;
}