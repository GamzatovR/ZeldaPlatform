using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Не пускает дальше, пока у пользователя не включён второй фактор.
///
/// Требование docs/SPEC.md §8.2: для роли Admin двухфакторная аутентификация
/// обязательна, и без неё вход в область администрирования закрыт. В Фазе 9 фильтр
/// вешается соглашением на всю область Admin; здесь он готов и проверен.
///
/// Признак берётся из claim'а, а не из UserManager: тот живёт в Infrastructure,
/// и правило 3 §5.2 держит его типы вне Web.
///
/// Неаутентифицированный пользователь не обрабатывается здесь намеренно — это забота
/// [Authorize], который отправит его на страницу входа. Фильтр отвечает только
/// за «вошёл, но второго фактора нет».
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireTwoFactorEnabledAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>Куда отправлять за настройкой второго фактора.</summary>
    public const string SetupPage = "/Account/Manage/TwoFactorAuthentication";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (user.HasClaim(AppClaimTypes.TwoFactorEnabled, AppClaimTypes.True))
        {
            return;
        }

        context.Result = new RedirectToPageResult(SetupPage, new { area = "Identity" });
    }
}