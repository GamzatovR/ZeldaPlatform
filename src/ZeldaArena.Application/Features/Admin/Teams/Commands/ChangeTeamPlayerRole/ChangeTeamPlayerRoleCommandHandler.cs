using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.ChangeTeamPlayerRole;

public sealed class ChangeTeamPlayerRoleCommandHandler(
    IRepository<Team> teams,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeTeamPlayerRoleCommand, Result>
{
    public async Task<Result> Handle(ChangeTeamPlayerRoleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var team = await teams.GetByIdAsync(request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        var result = DomainRules.Apply(() => team.ChangePlayerRole(request.PlayerId, request.Role));

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}