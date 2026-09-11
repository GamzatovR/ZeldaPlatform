using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamPlayerRole;

public sealed class ChangeMyTeamPlayerRoleCommandHandler(
    IRepository<Team> teamRepository,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeMyTeamPlayerRoleCommand, Result>
{
    public async Task<Result> Handle(ChangeMyTeamPlayerRoleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = await MyTeamAccess
            .LoadEditableAsync(teamRepository, entitlements, currentUser, request.TeamId, cancellationToken)
            .ConfigureAwait(false);

        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var team = access.Value;

        if (!team.ActiveRoster.Any(entry => entry.PlayerId == request.PlayerId))
        {
            return Result.Failure(EsportsErrors.PlayerNotFound);
        }

        team.ChangePlayerRole(request.PlayerId, request.Role);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}