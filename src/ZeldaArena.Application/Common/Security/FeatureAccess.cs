using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Common.Security;

/// <summary>Проверка платной функции у текущего пользователя внутри сценария.</summary>
public static class FeatureAccess
{
    public static async Task<bool> CurrentUserHasAsync(
        this IEntitlementService entitlements,
        ICurrentUserService currentUser,
        string featureCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entitlements);
        ArgumentNullException.ThrowIfNull(currentUser);

        return currentUser.UserId is { } userId
            && await entitlements.HasFeatureAsync(userId, featureCode, cancellationToken).ConfigureAwait(false);
    }
}