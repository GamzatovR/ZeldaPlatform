using System.Linq.Expressions;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Common.Models.Esports;

public static class MatchCardProjection
{
    public static readonly Expression<Func<Match, MatchCardDto>> Expression = match => new MatchCardDto
    {
        Id = match.Id,
        TournamentName = match.Tournament == null ? string.Empty : match.Tournament.Name,
        TournamentSlug = match.Tournament == null ? string.Empty : match.Tournament.Slug.Value,
        TeamAId = match.TeamAId,
        TeamAName = match.TeamA == null ? string.Empty : match.TeamA.Name,
        TeamASlug = match.TeamA == null ? string.Empty : match.TeamA.Slug.Value,
        TeamALogoPath = match.TeamA == null ? null : match.TeamA.LogoPath,
        TeamBId = match.TeamBId,
        TeamBName = match.TeamB == null ? string.Empty : match.TeamB.Name,
        TeamBSlug = match.TeamB == null ? string.Empty : match.TeamB.Slug.Value,
        TeamBLogoPath = match.TeamB == null ? null : match.TeamB.LogoPath,
        ScoreA = match.ScoreA,
        ScoreB = match.ScoreB,
        BestOf = match.BestOf,
        ScheduledAt = match.ScheduledAt,
        Status = match.Status,
        WinnerTeamId = match.WinnerTeamId,
    };
}