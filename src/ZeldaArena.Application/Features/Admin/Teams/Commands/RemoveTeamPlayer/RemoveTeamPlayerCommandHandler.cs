using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.RemoveTeamPlayer;

public sealed class RemoveTeamPlayerCommandHandler(
    IRepository<Team> teams,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveTeamPlayerCommand, Result>
{
    public async Task<Result> Handle(RemoveTeamPlayerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var team = await teams.GetByIdAsync(request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        var result = DomainRules.Apply(() => team.RemovePlayer(request.PlayerId, clock.UtcNow));

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}