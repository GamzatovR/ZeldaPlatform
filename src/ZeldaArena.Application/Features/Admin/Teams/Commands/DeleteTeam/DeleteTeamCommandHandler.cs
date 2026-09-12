using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.DeleteTeam;

public sealed class DeleteTeamCommandHandler(
    IRepository<Team> teams,
    IReadRepository<Match> matches,
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTeamCommand, Result>
{
    public async Task<Result> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var team = await teams.GetByIdAsync(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        var id = request.Id;

        var hasHistory = team.RosterEntries.Count > 0
            || await queryExecutor.AnyAsync(
                matches.Query().Where(match => match.TeamAId == id || match.TeamBId == id),
                cancellationToken)
            || await queryExecutor.AnyAsync(
                tournaments.Query()
                    .SelectMany(tournament => tournament.Participants)
                    .Where(participant => participant.TeamId == id),
                cancellationToken);

        if (hasHistory)
        {
            return Result.Failure(EsportsErrors.TeamHasHistory);
        }

        var logo = team.LogoPath;

        teams.Remove(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logo is not null)
        {
            await storage.DeleteAsync(logo, cancellationToken);
        }

        return Result.Success();
    }
}