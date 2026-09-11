using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

/// <summary>
/// Страница команды за фиксированное число запросов, независимо от размера состава
/// и истории: профиль, состав, два счётчика, последние и ближайшие матчи
/// (обязательная проверка на N+1, docs/SPEC.md §16).
///
/// Неодобренную команду видит только её владелец. Для остальных её нет — ответ
/// тот же, что на несуществующий адрес, чтобы по нему нельзя было перебирать
/// ещё не опубликованные команды (docs/adr/ADR-0008).
/// </summary>
public sealed class GetTeamBySlugQueryHandler(
    IReadRepository<Team> teams,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Player> players,
    IReadRepository<Match> matches,
    ICurrentUserService currentUser,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTeamBySlugQuery, TeamDetailsDto?>
{
    /// <summary>Сколько последних матчей составляют форму и историю.</summary>
    public const int RecentCount = 5;

    public const int UpcomingCount = 5;

    public async Task<TeamDetailsDto?> Handle(GetTeamBySlugQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Slug.TryFrom(request.Slug, out var slug) || slug is null)
        {
            return null;
        }

        var viewerId = currentUser.UserId;

        var team = await queryExecutor.FirstOrDefaultAsync(
            teams.Query()
                .Where(team => team.Slug == slug)
                .Where(team => team.IsApproved || (viewerId != null && team.OwnerUserId == viewerId))
                .Select(team => new TeamDetailsDto
                {
                    Id = team.Id,
                    Slug = team.Slug.Value,
                    Name = team.Name,
                    Tag = team.Tag,
                    LogoPath = team.LogoPath,
                    CountryCode = team.Country.Value,
                    Region = team.Region,
                    Rating = team.Rating,
                    FoundedAt = team.FoundedAt,
                    Description = team.Description,
                    IsApproved = team.IsApproved,
                    IsOwnedByViewer = viewerId != null && team.OwnerUserId == viewerId,
                }),
            cancellationToken).ConfigureAwait(false);

        if (team is null)
        {
            return null;
        }

        var roster = await queryExecutor
            .ToListAsync(TeamRosterQuery.Active(rosterEntries, players, team.Id), cancellationToken)
            .ConfigureAwait(false);

        var involved = matches.Query().Where(match => match.TeamAId == team.Id || match.TeamBId == team.Id);
        var finished = involved.Where(match => match.Status == MatchStatus.Finished);

        var played = await queryExecutor.CountAsync(finished, cancellationToken).ConfigureAwait(false);
        var wins = await queryExecutor
            .CountAsync(finished.Where(match => match.WinnerTeamId == team.Id), cancellationToken)
            .ConfigureAwait(false);

        var recent = await queryExecutor.ToListAsync(
            finished
                .OrderByDescending(match => match.ScheduledAt)
                .Take(RecentCount)
                .Select(MatchCardProjection.Expression),
            cancellationToken).ConfigureAwait(false);

        var upcoming = await queryExecutor.ToListAsync(
            involved
                .Where(match => match.Status == MatchStatus.Live
                    || match.Status == MatchStatus.Scheduled
                    || match.Status == MatchStatus.Postponed)
                .OrderBy(match => match.ScheduledAt)
                .Take(UpcomingCount)
                .Select(MatchCardProjection.Expression),
            cancellationToken).ConfigureAwait(false);

        return team with
        {
            Roster = roster,
            MatchesPlayed = played,
            Wins = wins,
            Form = [.. recent.Select(match => match.WinnerTeamId == team.Id)],
            RecentMatches = recent,
            UpcomingMatches = upcoming,
        };
    }
}