using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.AddTeamPlayer;

public sealed class AddTeamPlayerCommandHandler(
    IRepository<Team> teams,
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddTeamPlayerCommand, Result>
{
    public async Task<Result> Handle(AddTeamPlayerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var team = await teams.GetByIdAsync(request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        if (await players.FindAsync(request.PlayerId, cancellationToken) is null)
        {
            return Result.Failure(EsportsErrors.PlayerNotFound);
        }

        if (await queryExecutor.AnyAsync(
                rosterEntries.Query().Where(entry => entry.PlayerId == request.PlayerId && entry.LeftAt == null),
                cancellationToken))
        {
            return Result.Failure(EsportsErrors.PlayerInAnotherTeam);
        }

        var result = DomainRules.Apply(() => team.AddPlayer(request.PlayerId, request.Role, clock.UtcNow));

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}