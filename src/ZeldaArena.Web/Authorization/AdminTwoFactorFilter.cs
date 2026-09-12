using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Areas.Api;

namespace ZeldaArena.Web.Authorization;

/// <summary>Не пускает администратора в админку, пока у него не включён второй фактор.</summary>
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
        // для него — это 200 с чужой разметкой.
        context.Result = ApiRequest.Is(context.HttpContext.Request)
            ? new ForbidResult()
            : new RedirectToPageResult(SetupPage, new { area = "Identity" });
    }
}