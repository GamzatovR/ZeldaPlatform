using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Common.Security;

/// <summary>
/// Проверка платной функции у текущего пользователя внутри сценария. Источник истины —
/// по-прежнему <see cref="IEntitlementService"/> (docs/SPEC.md §7.3); здесь только
/// связка «кто сейчас работает» + «есть ли у него фича», чтобы каждый хендлер
/// не повторял разбор анонима.
///
/// Зачем проверять ещё и в сценарии, когда на действии уже висит <c>[RequireFeature]</c>:
/// атрибут защищает один вход — контроллер. Тот же сценарий вызовут эндпоинты
/// Areas/Api и мобильный клиент (EP-2), и без проверки здесь платная функция
/// открылась бы любому, кто найдёт второй вход.
/// </summary>
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