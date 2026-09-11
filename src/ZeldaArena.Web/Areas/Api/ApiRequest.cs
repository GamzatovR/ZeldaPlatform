using Microsoft.AspNetCore.Authentication.Cookies;

using ZeldaArena.Web.Areas.Api.Controllers;

namespace ZeldaArena.Web.Areas.Api;

/// <summary>
/// Запрос к <c>Areas/Api</c> отвечает кодом, а не страницей. Редирект на форму входа
/// для <c>fetch</c> — это 200 с HTML, который клиент примет за ответ сервиса
/// (docs/SPEC.md §10.1: «обработка 401/403/429 понятным тостом»).
/// </summary>
public static class ApiRequest
{
    public static bool Is(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Path.StartsWithSegments(ApiControllerBase.PathPrefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Неаутентифицированный запрос к API — 401, недостаточно прав — 403. Страницам
    /// оставлен прежний редирект на вход и на отказ в доступе.
    /// </summary>
    public static void AnswerApiWithStatusCodes(this CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Events.OnRedirectToLogin = context =>
            StatusOrRedirect(context.Request, context.Response, context.RedirectUri, StatusCodes.Status401Unauthorized);

        options.Events.OnRedirectToAccessDenied = context =>
            StatusOrRedirect(context.Request, context.Response, context.RedirectUri, StatusCodes.Status403Forbidden);
    }

    private static Task StatusOrRedirect(HttpRequest request, HttpResponse response, string redirectUri, int statusCode)
    {
        if (Is(request))
        {
            response.StatusCode = statusCode;
        }
        else
        {
            response.Redirect(redirectUri);
        }

        return Task.CompletedTask;
    }
}
