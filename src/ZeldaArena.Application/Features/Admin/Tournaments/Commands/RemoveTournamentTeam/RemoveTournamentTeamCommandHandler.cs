using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.RemoveTournamentTeam;

public sealed class RemoveTournamentTeamCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveTournamentTeamCommand, Result>
{
    public async Task<Result> Handle(RemoveTournamentTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await tournamentRepository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(EsportsErrors.TournamentNotFound);
        }

        var hasMatches = await queryExecutor.AnyAsync(
            matches.Query().Where(match => match.TournamentId == request.TournamentId
                && (match.TeamAId == request.TeamId || match.TeamBId == request.TeamId)),
            cancellationToken);

        if (hasMatches)
        {
            return Result.Failure(EsportsErrors.ParticipantHasMatches);
        }

        var result = DomainRules.Apply(() => tournament.RemoveTeam(request.TeamId));

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}