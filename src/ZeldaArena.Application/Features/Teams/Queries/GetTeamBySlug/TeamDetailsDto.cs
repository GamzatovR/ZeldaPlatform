using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

public sealed record TeamDetailsDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Tag { get; init; } = string.Empty;

    public string? LogoPath { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public Region Region { get; init; }

    public int Rating { get; init; }

    public DateOnly? FoundedAt { get; init; }

    public string? Description { get; init; }

    /// <summary>
    /// Команда подписчика до одобрения модератором. Такую страницу видит только
    /// владелец — с плашкой «на модерации» (docs/adr/ADR-0008).
    /// </summary>
    public bool IsApproved { get; init; }

    public bool IsOwnedByViewer { get; init; }

    public IReadOnlyList<RosterPlayerDto> Roster { get; init; } = [];

    public int MatchesPlayed { get; init; }

    public int Wins { get; init; }

    public int Losses => MatchesPlayed - Wins;

    /// <summary>Процент побед, округлённый до целого; без сыгранных матчей — ноль.</summary>
    public int WinRate => MatchesPlayed == 0 ? 0 : (int)Math.Round(Wins * 100m / MatchesPlayed);

    /// <summary>Исходы последних матчей, от свежего к старому: <c>true</c> — победа.</summary>
    public IReadOnlyList<bool> Form { get; init; } = [];

    public IReadOnlyList<MatchCardDto> RecentMatches { get; init; } = [];

    public IReadOnlyList<MatchCardDto> UpcomingMatches { get; init; } = [];
}