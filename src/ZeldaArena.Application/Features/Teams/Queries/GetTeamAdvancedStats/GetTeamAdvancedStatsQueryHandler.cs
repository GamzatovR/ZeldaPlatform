using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Security;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

/// <summary>
/// Сначала право, потом данные: без функции <c>stats.advanced</c> ни одного запроса
/// к статистике не уходит.
///
/// Агрегаты считает база (<c>GROUP BY</c>), имена подтягиваются вторым запросом
/// по готовому списку идентификаторов — четыре запроса на страницу, сколько бы
/// матчей ни сыграла команда (docs/SPEC.md §16).
/// </summary>
public sealed class GetTeamAdvancedStatsQueryHandler(
    IReadRepository<PlayerMatchStats> stats,
    IReadRepository<Player> players,
    IReadRepository<Match> matches,
    IReadRepository<Tournament> tournaments,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTeamAdvancedStatsQuery, Result<TeamAdvancedStatsDto>>
{
    public async Task<Result<TeamAdvancedStatsDto>> Handle(
        GetTeamAdvancedStatsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await entitlements
                .CurrentUserHasAsync(currentUser, FeatureCodes.StatsAdvanced, cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure<TeamAdvancedStatsDto>(EsportsErrors.FeatureRequired);
        }

        var averages = await queryExecutor.ToListAsync(
            stats.Query()
                .Where(line => line.TeamId == request.TeamId)
                .GroupBy(line => line.PlayerId)
                .Select(group => new
                {
                    PlayerId = group.Key,
                    Matches = group.Count(),
                    Kills = group.Average(line => (double)line.Kills),
                    Deaths = group.Average(line => (double)line.Deaths),
                    Assists = group.Average(line => (double)line.Assists),
                    Damage = group.Average(line => (double)line.Damage),
                    Rating = group.Average(line => (double)line.Rating),
                }),
            cancellationToken).ConfigureAwait(false);

        var playerIds = averages.Select(line => line.PlayerId).ToList();
        var names = await queryExecutor.ToListAsync(
            players.Query()
                .Where(player => playerIds.Contains(player.Id))
                .Select(player => new { player.Id, Slug = player.Slug.Value, player.Nickname }),
            cancellationToken).ConfigureAwait(false);

        var records = await queryExecutor.ToListAsync(
            matches.Query()
                .Where(match => match.Status == MatchStatus.Finished
                    && (match.TeamAId == request.TeamId || match.TeamBId == request.TeamId))
                .GroupBy(match => match.TournamentId)
                .Select(group => new
                {
                    TournamentId = group.Key,
                    Played = group.Count(),
                    Wins = group.Count(match => match.WinnerTeamId == request.TeamId),
                }),
            cancellationToken).ConfigureAwait(false);

        var tournamentIds = records.Select(record => record.TournamentId).ToList();
        var tournamentNames = await queryExecutor.ToListAsync(
            tournaments.Query()
                .Where(tournament => tournamentIds.Contains(tournament.Id))
                .Select(tournament => new
                {
                    tournament.Id,
                    Slug = tournament.Slug.Value,
                    tournament.Name,
                    tournament.StartsAt,
                }),
            cancellationToken).ConfigureAwait(false);

        return new TeamAdvancedStatsDto
        {
            Players =
            [
                .. from line in averages
                   join player in names on line.PlayerId equals player.Id
                   orderby line.Rating descending, player.Nickname
                   select new PlayerAveragesDto
                   {
                       PlayerId = line.PlayerId,
                       Slug = player.Slug,
                       Nickname = player.Nickname,
                       Matches = line.Matches,
                       Kills = line.Kills,
                       Deaths = line.Deaths,
                       Assists = line.Assists,
                       Damage = line.Damage,
                       Rating = line.Rating,
                   },
            ],
            Tournaments =
            [
                .. from record in records
                   join tournament in tournamentNames on record.TournamentId equals tournament.Id
                   orderby tournament.StartsAt descending
                   select new TournamentRecordDto
                   {
                       Slug = tournament.Slug,
                       Name = tournament.Name,
                       Played = record.Played,
                       Wins = record.Wins,
                   },
            ],
        };
    }
}