using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Security;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams;

internal static class MyTeamAccess
{
    public static async Task<Result<Team>> LoadEditableAsync(
        IRepository<Team> teams,
        IEntitlementService entitlements,
        ICurrentUserService currentUser,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<Team>(AccountErrors.UserNotFound);
        }

        if (!await entitlements.CurrentUserHasAsync(currentUser, FeatureCodes.TeamCreate, cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure<Team>(EsportsErrors.FeatureRequired);
        }

        var team = await teams.GetByIdAsync(teamId, cancellationToken).ConfigureAwait(false);

        return team is not null && team.OwnerUserId == userId
            ? team
            : Result.Failure<Team>(EsportsErrors.TeamNotFound);
    }
}