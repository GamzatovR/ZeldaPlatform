using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

/// <summary>
/// Турнир целиком для его страницы. <see cref="RulesHtml"/> — один из двух полей,
/// которые выводятся через <c>Html.Raw</c>: он проходит HtmlSanitizer на входе
/// (docs/SPEC.md §15), здесь его уже не чистят.
/// </summary>
public sealed record TournamentDetailsDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public TournamentTier Tier { get; init; }

    public Region Region { get; init; }

    public decimal PrizePoolAmount { get; init; }

    public string PrizePoolCurrency { get; init; } = string.Empty;

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public TournamentStatus Status { get; init; }

    public string? RulesHtml { get; init; }

    public string? LogoPath { get; init; }

    public string? BannerPath { get; init; }

    public bool IsFeatured { get; init; }

    public IReadOnlyList<TournamentParticipantDto> Participants { get; init; } = [];
}