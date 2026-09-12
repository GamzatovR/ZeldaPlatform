using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.SetTeamApproval;

public sealed class SetTeamApprovalCommandHandler(
    IRepository<Team> teams,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetTeamApprovalCommand, Result>
{
    public async Task<Result> Handle(SetTeamApprovalCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var team = await teams.GetByIdAsync(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        if (request.IsApproved)
        {
            team.Approve();
        }
        else
        {
            team.Revoke();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}