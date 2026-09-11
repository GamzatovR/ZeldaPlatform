using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

public sealed record PlayerDetailsDto
{
    public Guid Id { get; init; }

    public string Slug { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public DateOnly? BirthDate { get; init; }

    public PlayerRole Role { get; init; }

    public string? AvatarPath { get; init; }

    public string? Bio { get; init; }

    /// <summary>Вся история составов, от свежей записи к старой; открытая запись — текущая команда.</summary>
    public IReadOnlyList<PlayerTeamHistoryDto> Teams { get; init; } = [];

    public PlayerTeamHistoryDto? CurrentTeam => Teams.FirstOrDefault(team => team.LeftAt is null);

    public int MatchesPlayed { get; init; }

    public double AverageKills { get; init; }

    public double AverageDeaths { get; init; }

    public double AverageAssists { get; init; }

    public double AverageRating { get; init; }

    public IReadOnlyList<MatchCardDto> RecentMatches { get; init; } = [];
}