using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Areas.Api;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Не пускает администратора в админку, пока у него не включён второй фактор.
///
/// Требование docs/SPEC.md §8.2: «для роли Admin 2FA обязательна, без неё вход в Area
/// Admin заблокирован фильтром». Модератор проходит без второго фактора — решение
/// пользователя в Фазе 9 (docs/adr/ADR-0010): §8.2 говорит о роли Admin, а не обо всех,
/// кто работает в области. На область фильтр вешает <c>AdminAreaConvention</c>.
///
/// Признак берётся из claim'а, а не из UserManager: тот живёт в Infrastructure,
/// и правило 3 §5.2 держит его типы вне Web. Claim перевыпускается при включении
/// и отключении 2FA (Фаза 3), поэтому снимок в cookie не отстаёт.
///
/// Неаутентифицированный пользователь не обрабатывается здесь намеренно — это забота
/// политики области, которая отправит его на страницу входа. Фильтр отвечает только
/// за «вошёл как администратор, но второго фактора нет».
/// </summary>
public sealed class AdminTwoFactorFilter : IAuthorizationFilter
{
    /// <summary>Куда отправлять за настройкой второго фактора.</summary>
    public const string SetupPage = "/Account/Manage/TwoFactorAuthentication";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true
            || !user.IsInRole(RoleNames.Admin)
            || user.HasClaim(AppClaimTypes.TwoFactorEnabled, AppClaimTypes.True))
        {
            return;
        }

        // fetch из админки получает 403, а не HTML страницы настройки 2FA: редирект
        // для него — это 200 с чужой разметкой (так же отвечает весь Areas/Api, §10.1).
        context.Result = ApiRequest.Is(context.HttpContext.Request)
            ? new ForbidResult()
            : new RedirectToPageResult(SetupPage, new { area = "Identity" });
    }
}