using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Security;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;

/// <summary>Сначала право, потом данные — как у расширенной статистики команды.</summary>
public sealed class GetPlayerAdvancedStatsQueryHandler(
    IReadRepository<PlayerMatchStats> stats,
    IReadRepository<Match> matches,
    IReadRepository<Tournament> tournaments,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetPlayerAdvancedStatsQuery, Result<IReadOnlyList<PlayerTournamentStatsDto>>>
{
    public async Task<Result<IReadOnlyList<PlayerTournamentStatsDto>>> Handle(
        GetPlayerAdvancedStatsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await entitlements
                .CurrentUserHasAsync(currentUser, FeatureCodes.StatsAdvanced, cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure<IReadOnlyList<PlayerTournamentStatsDto>>(EsportsErrors.FeatureRequired);
        }

        var byTournament = await queryExecutor.ToListAsync(
            from line in stats.Query()
            join match in matches.Query() on line.MatchId equals match.Id
            where line.PlayerId == request.PlayerId
            group line by match.TournamentId into tournament
            select new
            {
                TournamentId = tournament.Key,
                Matches = tournament.Count(),
                Kills = tournament.Average(line => (double)line.Kills),
                Deaths = tournament.Average(line => (double)line.Deaths),
                Assists = tournament.Average(line => (double)line.Assists),
                Damage = tournament.Average(line => (double)line.Damage),
                Rating = tournament.Average(line => (double)line.Rating),
            },
            cancellationToken).ConfigureAwait(false);

        var ids = byTournament.Select(line => line.TournamentId).ToList();
        var names = await queryExecutor.ToListAsync(
            tournaments.Query()
                .Where(tournament => ids.Contains(tournament.Id))
                .Select(tournament => new
                {
                    tournament.Id,
                    Slug = tournament.Slug.Value,
                    tournament.Name,
                    tournament.StartsAt,
                }),
            cancellationToken).ConfigureAwait(false);

        IReadOnlyList<PlayerTournamentStatsDto> result =
        [
            .. from line in byTournament
               join tournament in names on line.TournamentId equals tournament.Id
               orderby tournament.StartsAt descending
               select new PlayerTournamentStatsDto
               {
                   Slug = tournament.Slug,
                   Name = tournament.Name,
                   Matches = line.Matches,
                   Kills = line.Kills,
                   Deaths = line.Deaths,
                   Assists = line.Assists,
                   Damage = line.Damage,
                   Rating = line.Rating,
               },
        ];

        return Result.Success(result);
    }
}