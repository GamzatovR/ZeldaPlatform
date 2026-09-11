using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Security;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams;

/// <summary>
/// Общий вход всех сценариев правки своей команды: кто работает, есть ли у него
/// функция <c>team.create</c> и его ли это команда.
///
/// Чужая команда и несуществующая дают один и тот же ответ «команды нет»: иначе по
/// ответу можно было бы перебирать чужие идентификаторы (защита от IDOR, docs/SPEC.md §15).
/// Функция проверяется здесь, а не только атрибутом действия: после истечения
/// подписки править команду нельзя ни с какого входа (CLAUDE.md, «При неопределённости»).
/// </summary>
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