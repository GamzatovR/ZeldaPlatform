using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.RemovePlayerFromMyTeam;

public sealed class RemovePlayerFromMyTeamCommandHandler(
    IRepository<Team> teamRepository,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePlayerFromMyTeamCommand, Result>
{
    public async Task<Result> Handle(RemovePlayerFromMyTeamCommand request, CancellationToken cancellationToken)
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

        // Игрок не из этого состава — «игрока нет», а не исключение домена и ошибка сервера.
        if (!team.ActiveRoster.Any(entry => entry.PlayerId == request.PlayerId))
        {
            return Result.Failure(EsportsErrors.PlayerNotFound);
        }

        team.RemovePlayer(request.PlayerId, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}