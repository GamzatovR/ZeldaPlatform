using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

public sealed class GetMatchFormOptionsQueryHandler(
    IReadRepository<Tournament> tournaments,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMatchFormOptionsQuery, MatchFormOptionsDto>
{
    public async Task<MatchFormOptionsDto> Handle(GetMatchFormOptionsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var open = await queryExecutor.ToListAsync(
            tournaments.Query()
                .Where(tournament => tournament.Status == TournamentStatus.Announced
                    || tournament.Status == TournamentStatus.Ongoing)
                .OrderByDescending(tournament => tournament.StartsAt)
                .Select(tournament => new AdminTournamentOptionDto(tournament.Id, tournament.Name)),
            cancellationToken);

        if (request.TournamentId is not { } tournamentId)
        {
            return new MatchFormOptionsDto { Tournaments = open };
        }

        var participants = await queryExecutor.ToListAsync(
            from participant in tournaments.Query()
                .Where(tournament => tournament.Id == tournamentId)
                .SelectMany(tournament => tournament.Participants)
            join team in teams.Query() on participant.TeamId equals team.Id
            orderby team.Name
            select new AdminTeamOptionDto(team.Id, team.Name),
            cancellationToken);

        return new MatchFormOptionsDto { Tournaments = open, Teams = participants };
    }
}