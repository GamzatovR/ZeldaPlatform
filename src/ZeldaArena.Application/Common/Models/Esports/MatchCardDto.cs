using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Common.Models.Esports;

public sealed record MatchCardDto
{
    public Guid Id { get; init; }

    public string TournamentName { get; init; } = string.Empty;

    public string TournamentSlug { get; init; } = string.Empty;

    public Guid TeamAId { get; init; }

    public string TeamAName { get; init; } = string.Empty;

    public string TeamASlug { get; init; } = string.Empty;

    public string? TeamALogoPath { get; init; }

    public Guid TeamBId { get; init; }

    public string TeamBName { get; init; } = string.Empty;

    public string TeamBSlug { get; init; } = string.Empty;

    public string? TeamBLogoPath { get; init; }

    public int ScoreA { get; init; }

    public int ScoreB { get; init; }

    public int BestOf { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public MatchStatus Status { get; init; }

    public Guid? WinnerTeamId { get; init; }

    public bool IsLive => Status == MatchStatus.Live;
}